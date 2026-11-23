"""Gera os arquivos JSON de seed do PortalCopa26 a partir das fontes do protótipo.

Uso:
    python -I gerar_seed.py <pasta-do-prototipo> <pasta-de-saida>

Exemplo (a partir da raiz do workspace):
    python -I tools/seed-generator/gerar_seed.py ^
        C:/Users/kendj/wrk/world-cup/prototipo ^
        src/PortalCopa26/PortalCopa26/Data/Seed

Fontes de verdade: prototipo/fontes/*.txt.
De prototipo/js/site.js são usados apenas os códigos FIFA por grupo (validação
cruzada) e o campo `copas` dos jogadores.

O script falha (exit code 1) se encontrar seleção ou cidade sem mapeamento, ou se
as contagens esperadas (12 grupos, 48 seleções, 16 estádios, 104 jogos, 98 entradas
de ranking) não forem atingidas.
"""

from __future__ import annotations

import json
import re
import sys
import unicodedata
from datetime import datetime, timedelta, timezone
from pathlib import Path

# Horários das fontes estão em horário de Brasília (UTC-3, sem horário de verão em 2026).
OFFSET_BRASILIA = timedelta(hours=-3)
ANO = 2026
MESES = {"junho": 6, "julho": 7}


class ErroSeed(Exception):
    pass


def normalizar(texto: str) -> str:
    """Minúsculas, sem acentos e com espaços simples."""
    sem_acento = unicodedata.normalize("NFKD", texto)
    sem_acento = "".join(c for c in sem_acento if not unicodedata.combining(c))
    return re.sub(r"\s+", " ", sem_acento).strip().lower()


# --------------------------------------------------------------------------- #
# Mapas de aliases (design D8)
# --------------------------------------------------------------------------- #

ALIASES_SELECAO: dict[str, str] = {
    normalizar(nome): codigo
    for codigo, nomes in {
        "MEX": ["México"],
        "RSA": ["África do Sul"],
        "KOR": ["Coreia do Sul", "República da Coreia"],
        "CZE": ["República Tcheca", "Tchéquia"],
        "CAN": ["Canadá"],
        "BIH": ["Bósnia e Herzegovina", "Bósnia"],
        "QAT": ["Catar"],
        "SUI": ["Suíça"],
        "BRA": ["Brasil"],
        "MAR": ["Marrocos"],
        "HAI": ["Haiti"],
        "SCO": ["Escócia"],
        "USA": ["Estados Unidos", "EUA"],
        "PAR": ["Paraguai"],
        "AUS": ["Austrália"],
        "TUR": ["Turquia"],
        "GER": ["Alemanha"],
        "CUW": ["Curaçao", "Curaçau"],
        "CIV": ["Costa do Marfim", "Côte d'Ivoire"],
        "ECU": ["Equador"],
        "NED": ["Holanda", "Países Baixos"],
        "JPN": ["Japão"],
        "SWE": ["Suécia"],
        "TUN": ["Tunísia"],
        "BEL": ["Bélgica"],
        "EGY": ["Egito"],
        "IRN": ["Irã", "República Islâmica do Irã"],
        "NZL": ["Nova Zelândia"],
        "ESP": ["Espanha"],
        "CPV": ["Cabo Verde"],
        "KSA": ["Arábia Saudita"],
        "URU": ["Uruguai"],
        "FRA": ["França"],
        "SEN": ["Senegal"],
        "IRQ": ["Iraque"],
        "NOR": ["Noruega"],
        "ARG": ["Argentina"],
        "ALG": ["Argélia"],
        "AUT": ["Áustria"],
        "JOR": ["Jordânia"],
        "POR": ["Portugal"],
        "COD": ["RD Congo", "ReD do Congo", "República Democrática do Congo"],
        "UZB": ["Uzbequistão"],
        "COL": ["Colômbia"],
        "ENG": ["Inglaterra"],
        "CRO": ["Croácia"],
        "GHA": ["Gana"],
        "PAN": ["Panamá"],
    }.items()
    for nome in nomes
}

# Grafia de cidade nas fontes de jogos -> cidade da tabela de estádios.
ALIASES_CIDADE: dict[str, str] = {
    normalizar(k): v
    for k, v in {
        "Santa Clara": "San Francisco",
        "Nova Iorque": "Nova York/New Jersey",
        "Nova Iorque/Nova Jersey": "Nova York/New Jersey",
        "Nova Jersey": "Nova York/New Jersey",
        "Nova York/Nova Jersey": "Nova York/New Jersey",
        "Azteca": "Cidade do México",
        "Seattle Field": "Seattle",
        "Vancouver Place": "Vancouver",
    }.items()
}

PAIS_SEDE = {"mexico": "Mexico", "eua": "EstadosUnidos", "canada": "Canada"}

POSICAO = {
    "goleiro": "Goleiro",
    "defensor": "Defensor",
    "meio-campista": "MeioCampista",
    "atacante": "Atacante",
}

# Participantes que não constam do arquivo de ranking (verificado nas fontes).
AUSENTES_DO_RANKING = {"CUW", "CPV", "UZB", "NZL", "GHA", "BIH", "KSA"}


def codigo_selecao(nome: str, origem: str) -> str:
    codigo = ALIASES_SELECAO.get(normalizar(nome))
    if codigo is None:
        raise ErroSeed(f"Seleção sem código FIFA mapeado: '{nome}' (em {origem})")
    return codigo


def ler(caminho: Path) -> list[str]:
    return caminho.read_text(encoding="utf-8").replace("\r\n", "\n").split("\n")


# --------------------------------------------------------------------------- #
# Grupos e seleções
# --------------------------------------------------------------------------- #

def ler_grupos(fontes: Path) -> list[dict]:
    """Lê apenas as linhas A..L de copa2026_grupos.txt (o arquivo tem TABs
    irregulares e um trecho de PRD colado após o grupo L)."""
    grupos: list[dict] = []
    for linha in ler(fontes / "copa2026_grupos.txt"):
        m = re.match(r"^([A-L])\t(.*)$", linha)
        if not m:
            continue
        letra, resto = m.groups()
        tokens = [t.strip() for t in resto.split("\t") if t.strip()]
        selecoes: list[tuple[str, str]] = []  # (codigo, nome exibido)
        i = 0
        while i < len(tokens):
            # Tenta o token sozinho e, se não casar, junta com os seguintes
            # (caso "Estados" + "Unidos").
            for tamanho in range(1, len(tokens) - i + 1):
                nome = " ".join(tokens[i : i + tamanho])
                if normalizar(nome) in ALIASES_SELECAO:
                    selecoes.append((ALIASES_SELECAO[normalizar(nome)], nome))
                    i += tamanho
                    break
            else:
                raise ErroSeed(f"Grupo {letra}: seleção sem código FIFA: '{tokens[i]}'")
        if len(selecoes) != 4:
            raise ErroSeed(f"Grupo {letra} tem {len(selecoes)} seleções (esperado 4)")
        grupos.append({"letra": letra, "selecoes": selecoes})
        if letra == "L":
            break
    if [g["letra"] for g in grupos] != list("ABCDEFGHIJKL"):
        raise ErroSeed(f"Grupos lidos: {[g['letra'] for g in grupos]} (esperado A..L)")
    return grupos


def ler_grupos_site_js(site_js: str) -> dict[str, set[str]]:
    return {
        letra: set(re.findall(r"'([A-Z]{3})'", times))
        for letra, times in re.findall(r"([A-L]): \{ teams: \[([^\]]*)\]", site_js)
    }


def ler_tecnicos(fontes: Path) -> dict[str, str]:
    tecnicos: dict[str, str] = {}
    for linha in ler(fontes / "copa2026_pais_tecnicos.txt"):
        if "|" not in linha:
            continue
        pais, tecnico = (p.strip() for p in linha.split("|", 1))
        tecnicos[codigo_selecao(pais, "copa2026_pais_tecnicos.txt")] = tecnico
    return tecnicos


# --------------------------------------------------------------------------- #
# Estádios
# --------------------------------------------------------------------------- #

def ler_estadios(fontes: Path) -> list[dict]:
    estadios: list[dict] = []
    for linha in ler(fontes / "copa2026_cidades_sede_estadios.txt"):
        colunas = [c.strip() for c in linha.strip().strip("|").split("|")]
        if len(colunas) != 4 or colunas[0] in ("Cidade", "") or set(colunas[0]) <= set("-"):
            continue
        cidade, pais, nome, capacidade = colunas
        pais_enum = PAIS_SEDE.get(normalizar(pais))
        if pais_enum is None:
            raise ErroSeed(f"País-sede desconhecido: '{pais}'")
        estadios.append(
            {
                "nome": nome,
                "cidade": cidade,
                "pais": pais_enum,
                "capacidade": int(re.sub(r"\D", "", capacidade)),  # "30.000*" -> 30000
            }
        )
    return estadios


def cidade_estadio(cidade_fonte: str, estadios: list[dict], origem: str) -> str:
    chave = normalizar(cidade_fonte)
    alvo = ALIASES_CIDADE.get(chave, cidade_fonte)
    encontrados = [e["cidade"] for e in estadios if normalizar(e["cidade"]) == normalizar(alvo)]
    if len(encontrados) != 1:
        raise ErroSeed(f"Cidade sem estádio mapeado: '{cidade_fonte}' (em {origem})")
    return encontrados[0]


# --------------------------------------------------------------------------- #
# Jogos
# --------------------------------------------------------------------------- #

def para_utc(dia: int, mes: int, hora: int, minuto: int) -> datetime:
    local = datetime(ANO, mes, dia, hora, minuto, tzinfo=timezone(OFFSET_BRASILIA))
    return local.astimezone(timezone.utc)


def ler_jogos_grupos(fontes: Path, estadios: list[dict]) -> list[dict]:
    origem = "copa2026_jogos_primeira_fase.txt"
    linhas = ler(fontes / origem)
    jogos: list[dict] = []
    data_atual: tuple[int, int] | None = None
    i = 0
    while i < len(linhas):
        linha = linhas[i].strip()
        cabecalho = re.match(r"^\S+ (\d{1,2}) (\w+) 2026$", linha)
        if cabecalho:
            data_atual = (int(cabecalho.group(1)), MESES[cabecalho.group(2)])
        partida = re.match(
            r"^(.+?) x (.+?)\s+(\d{2}):(\d{2}) hs(?: \((\d{1,2}) de (\w+)\))?$", linha
        )
        if partida:
            if data_atual is None:
                raise ErroSeed(f"Jogo antes de qualquer cabeçalho de data: '{linha}'")
            mandante, visitante, hh, mm, dia_alt, mes_alt = partida.groups()
            dia, mes = (int(dia_alt), MESES[mes_alt]) if dia_alt else data_atual
            detalhe = linhas[i + 1].strip()
            info = re.match(r"^Primeira fase · Grupo ([A-L]) · .*\(([^)]+)\)$", detalhe)
            if not info:
                raise ErroSeed(f"Linha de detalhe inesperada após '{linha}': '{detalhe}'")
            grupo, cidade = info.groups()
            jogos.append(
                {
                    "fase": "Grupos",
                    "rotulo": None,
                    "grupo": grupo,
                    "cidade": cidade_estadio(cidade, estadios, origem),
                    "utc": para_utc(dia, mes, int(hh), int(mm)),
                    "mandante": codigo_selecao(mandante, origem),
                    "visitante": codigo_selecao(visitante, origem),
                    "descricaoMandante": None,
                    "descricaoVisitante": None,
                }
            )
            i += 2
            continue
        i += 1
    return jogos


ARQUIVOS_MATA_MATA = [
    ("copa2026_Jogos_Segunda_fase.txt", "SegundaFase"),
    ("copa2026_jogos_oitavas.txt", "Oitavas"),
    ("copa2026_jogos_quartas.txt", "Quartas"),
    ("copa2026_jogos_semifinal.txt", "Semifinal"),
    ("copa2026_jogo_terceiro_lugar.txt", "TerceiroLugar"),
    ("copa2026_jogo_final.txt", "Final"),
]


def lado_do_confronto(texto: str, origem: str) -> tuple[str | None, str | None]:
    """Retorna (codigo, descricao): seleção definida ou descrição de vaga."""
    texto = texto.strip()
    if re.match(r"^(Venc|Perd)\.", texto):
        return None, re.sub(r"\s+", " ", texto)
    return codigo_selecao(texto, origem), None


def ler_jogos_mata_mata(fontes: Path, estadios: list[dict]) -> list[dict]:
    jogos: list[dict] = []
    for arquivo, fase in ARQUIVOS_MATA_MATA:
        texto = (fontes / arquivo).read_text(encoding="utf-8").replace("\r\n", "\n")
        blocos = [b for b in re.split(r"\n\s*\n", texto) if b.strip()]
        for bloco in blocos:
            linhas = [l.strip() for l in bloco.split("\n") if l.strip()]
            rotulo = linhas[0]
            corpo = "\n".join(linhas[1:])
            m_data = re.search(r"^(.*?)\s+(\d{2})/(\d{2})", corpo, re.MULTILINE)
            m_hora = re.search(r"(\d{2}):(\d{2})", corpo)
            confronto = next((l for l in linhas[1:] if re.search(r"\sx\s", l)), None)
            if not (m_data and m_hora and confronto):
                raise ErroSeed(f"Bloco de mata-mata inválido em {arquivo}: {linhas}")
            cidade_fonte, dia, mes = m_data.group(1), int(m_data.group(2)), int(m_data.group(3))
            esquerda, direita = re.split(r"\s+x\s+", confronto, maxsplit=1)
            mandante, desc_mandante = lado_do_confronto(esquerda, arquivo)
            visitante, desc_visitante = lado_do_confronto(direita, arquivo)
            jogos.append(
                {
                    "fase": fase,
                    "rotulo": re.sub(r"\s+", " ", rotulo),
                    "grupo": None,
                    "cidade": cidade_estadio(cidade_fonte, estadios, arquivo),
                    "utc": para_utc(dia, mes, int(m_hora.group(1)), int(m_hora.group(2))),
                    "mandante": mandante,
                    "visitante": visitante,
                    "descricaoMandante": desc_mandante,
                    "descricaoVisitante": desc_visitante,
                }
            )
    return jogos


# --------------------------------------------------------------------------- #
# Jogadores e ranking
# --------------------------------------------------------------------------- #

def ler_copas_site_js(site_js: str) -> dict[str, dict[str, int]]:
    """codigo -> {nome normalizado -> copas}."""
    copas: dict[str, dict[str, int]] = {}
    atual: str | None = None
    for linha in site_js.split("\n"):
        selecao = re.match(r"^  ([A-Z]{3}): \{ nome:", linha)
        if selecao:
            atual = selecao.group(1)
            copas.setdefault(atual, {})
            continue
        jogador = re.search(r"\{ nome: '([^']+)',.*copas:\s*(\d+)", linha)
        if jogador and atual:
            copas[atual][normalizar(jogador.group(1))] = int(jogador.group(2))
    return copas


def ler_jogadores(fontes: Path, copas: dict[str, dict[str, int]]) -> tuple[list[dict], int]:
    origem = "copa2026_selecoes_jogadores.txt"
    jogadores: list[dict] = []
    com_copas = 0
    atual: str | None = None
    for linha in ler(fontes / origem):
        linha = linha.strip()
        if not linha:
            continue
        if linha.startswith("#"):
            atual = codigo_selecao(linha.lstrip("#").strip(), origem)
            continue
        partes = [p.strip() for p in linha.split("|")]
        if len(partes) != 4 or atual is None:
            raise ErroSeed(f"Linha de jogador inválida: '{linha}'")
        nome, idade, posicao, gols = partes
        posicao_enum = POSICAO.get(normalizar(posicao))
        if posicao_enum is None:
            raise ErroSeed(f"Posição desconhecida: '{posicao}' ({nome})")
        participacoes = copas.get(atual, {}).get(normalizar(nome))
        if participacoes is not None:
            com_copas += 1
        jogadores.append(
            {
                "selecao": atual,
                "nome": nome,
                "posicao": posicao_enum,
                "idade": int(idade),
                "gols": int(gols),
                "participacoesCopas": participacoes or 0,
            }
        )
    return jogadores, com_copas


def ler_ranking(fontes: Path, participantes: set[str]) -> list[dict]:
    ranking: list[dict] = []
    for linha in ler(fontes / "copa2026_ranking_fifa.txt"):
        # O separador entre nome e pontos nem sempre existe ("República Tcheca1510.15").
        m = re.match(r"^(\d+)\s+(.+?)\s*(\d+\.\d+)\s*$", linha)
        if not m:
            continue
        posicao, nome, pontos = m.groups()
        codigo = ALIASES_SELECAO.get(normalizar(nome))
        ranking.append(
            {
                "posicao": int(posicao),
                "nomeSelecao": nome.strip(),
                "pontos": float(pontos),
                "selecao": codigo if codigo in participantes else None,
            }
        )
    return ranking


# --------------------------------------------------------------------------- #
# Montagem, validação e escrita
# --------------------------------------------------------------------------- #

def exigir(condicao: bool, mensagem: str) -> None:
    if not condicao:
        raise ErroSeed(mensagem)


def gerar(prototipo: Path, saida: Path) -> None:
    fontes = prototipo / "fontes"
    site_js = (prototipo / "js" / "site.js").read_text(encoding="utf-8")

    grupos = ler_grupos(fontes)
    grupos_site = ler_grupos_site_js(site_js)
    for g in grupos:
        codigos = {c for c, _ in g["selecoes"]}
        exigir(
            codigos == grupos_site.get(g["letra"], set()),
            f"Grupo {g['letra']}: códigos {sorted(codigos)} divergem do site.js "
            f"{sorted(grupos_site.get(g['letra'], set()))}",
        )

    tecnicos = ler_tecnicos(fontes)
    selecoes = []
    for g in grupos:
        for indice, (codigo, nome) in enumerate(g["selecoes"]):
            exigir(codigo in tecnicos, f"Seleção {codigo} sem técnico")
            selecoes.append(
                {
                    "codigoFifa": codigo,
                    "nome": nome,
                    "tecnico": tecnicos[codigo],
                    "grupo": g["letra"],
                    "cabecaDeChave": indice == 0,
                }
            )
    participantes = {s["codigoFifa"] for s in selecoes}
    grupo_de = {s["codigoFifa"]: s["grupo"] for s in selecoes}

    estadios = ler_estadios(fontes)

    jogos_grupos = sorted(ler_jogos_grupos(fontes, estadios), key=lambda j: j["utc"])
    for j in jogos_grupos:
        exigir(
            grupo_de[j["mandante"]] == j["grupo"] == grupo_de[j["visitante"]],
            f"Jogo {j['mandante']} x {j['visitante']} não pertence ao grupo {j['grupo']}",
        )
    ordem_fase = {fase: i for i, (_, fase) in enumerate(ARQUIVOS_MATA_MATA)}
    jogos_mata = sorted(
        ler_jogos_mata_mata(fontes, estadios), key=lambda j: (ordem_fase[j["fase"]], j["utc"])
    )
    jogos = []
    for numero, j in enumerate(jogos_grupos + jogos_mata, start=1):
        jogos.append(
            {
                "numero": numero,
                "fase": j["fase"],
                "rotulo": j["rotulo"],
                "grupo": j["grupo"],
                "cidadeEstadio": j["cidade"],
                "dataHoraUtc": j["utc"].strftime("%Y-%m-%dT%H:%M:%SZ"),
                "mandante": j["mandante"],
                "visitante": j["visitante"],
                "descricaoMandante": j["descricaoMandante"],
                "descricaoVisitante": j["descricaoVisitante"],
            }
        )

    jogadores, com_copas = ler_jogadores(fontes, ler_copas_site_js(site_js))
    ranking = ler_ranking(fontes, participantes)

    # ---- Validações (spec seed-data) ----
    exigir(len(grupos) == 12, f"{len(grupos)} grupos")
    exigir(len(selecoes) == 48, f"{len(selecoes)} seleções")
    exigir(sum(s["cabecaDeChave"] for s in selecoes) == 12, "cabeças de chave != 12")
    exigir(len(estadios) == 16, f"{len(estadios)} estádios")
    exigir(len(jogos_grupos) == 72, f"{len(jogos_grupos)} jogos da fase de grupos")
    exigir(len(jogos) == 104, f"{len(jogos)} jogos no total")
    for letra in "ABCDEFGHIJKL":
        do_grupo = [j for j in jogos if j["grupo"] == letra]
        pares = {frozenset((j["mandante"], j["visitante"])) for j in do_grupo}
        exigir(len(do_grupo) == 6 and len(pares) == 6, f"Grupo {letra}: confrontos incompletos")
    exigir(
        {j["cidadeEstadio"] for j in jogos} == {e["cidade"] for e in estadios},
        "Nem todos os 16 estádios recebem jogos",
    )
    segunda = [j for j in jogos if j["fase"] == "SegundaFase"]
    exigir(len(segunda) == 16 and all(j["mandante"] and j["visitante"] for j in segunda),
           "Segunda Fase deve ter 16 jogos com seleções definidas")
    exigir(all(j["mandante"] is None and j["visitante"] is None
               for j in jogos if j["fase"] not in ("Grupos", "SegundaFase")),
           "Jogos após a Segunda Fase devem usar descrições de vaga")
    exigir({j["selecao"] for j in jogadores} == participantes, "Nem todas as seleções têm elenco")
    exigir(len(ranking) == 98, f"{len(ranking)} entradas no ranking")
    vinculados = [r["selecao"] for r in ranking if r["selecao"]]
    exigir(len(vinculados) == len(set(vinculados)), "Seleção vinculada a mais de uma entrada do ranking")
    exigir(participantes - set(vinculados) == AUSENTES_DO_RANKING,
           f"Participantes sem ranking: {sorted(participantes - set(vinculados))}")

    # ---- Escrita ----
    saida.mkdir(parents=True, exist_ok=True)
    arquivos = {
        "grupos.json": [{"codigo": g["letra"]} for g in grupos],
        "selecoes.json": selecoes,
        "estadios.json": estadios,
        "jogos.json": jogos,
        "jogadores.json": jogadores,
        "ranking.json": ranking,
    }
    for nome, dados in arquivos.items():
        (saida / nome).write_text(
            json.dumps(dados, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
        )

    print(f"Grupos: {len(grupos)} | Seleções: {len(selecoes)} | Estádios: {len(estadios)}")
    print(f"Jogos: {len(jogos)} (grupos {len(jogos_grupos)}, mata-mata {len(jogos_mata)})")
    print(f"Jogadores: {len(jogadores)} ({com_copas} com participações em Copas vindas do site.js)")
    print(f"Ranking: {len(ranking)} entradas, {len(vinculados)} vinculadas a participantes")
    print(f"Arquivos gravados em {saida.resolve()}")


def main(argv: list[str]) -> int:
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")
    if len(argv) != 3:
        print(__doc__)
        return 2
    try:
        gerar(Path(argv[1]), Path(argv[2]))
    except ErroSeed as erro:
        print(f"ERRO: {erro}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
