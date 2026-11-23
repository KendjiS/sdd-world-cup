"""Verificação do banco SQLite do PortalCopa26.

Uso:
    python verificar_banco.py <caminho.db> contagens
    python verificar_banco.py <caminho.db> simulacao-exemplo
    python verificar_banco.py <caminho.db> restricoes

- contagens:          imprime o número de linhas de cada tabela da aplicação.
- simulacao-exemplo:  insere uma Simulacao com dois SimulacaoJogo (persistente).
- restricoes:         testa duplicidade de SimulacaoJogo, cascade na exclusão de
                      Simulacao e bloqueio da exclusão de Jogo referenciado.
                      Tudo é feito numa transação revertida ao final.

Execute com a aplicação parada, para não disputar o arquivo do banco.
"""

import sqlite3
import sys
from datetime import datetime, timezone

TABELAS = [
    "Grupos",
    "Selecoes",
    "Estadios",
    "Jogos",
    "Jogadores",
    "RankingFifa",
    "Simulacoes",
    "SimulacaoJogos",
]


def conectar(caminho: str) -> sqlite3.Connection:
    conexao = sqlite3.connect(caminho, isolation_level=None)
    # O EF Core ativa as foreign keys por conexão; aqui é preciso fazer o mesmo.
    conexao.execute("PRAGMA foreign_keys = ON")
    return conexao


def contagens(conexao: sqlite3.Connection) -> int:
    for tabela in TABELAS:
        (total,) = conexao.execute(f'SELECT COUNT(*) FROM "{tabela}"').fetchone()
        print(f"{tabela:<16} {total}")
    return 0


def _agora() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S")


def _dois_jogos(conexao: sqlite3.Connection) -> list[int]:
    ids = [r[0] for r in conexao.execute('SELECT Id FROM "Jogos" ORDER BY Numero LIMIT 2')]
    if len(ids) < 2:
        raise SystemExit("ERRO: o banco precisa ter ao menos 2 jogos (rode o seed antes).")
    return ids


def _criar_simulacao(conexao: sqlite3.Connection, nome: str, jogos: list[int]) -> int:
    agora = _agora()
    cursor = conexao.execute(
        'INSERT INTO "Simulacoes" (Nome, CriadaEmUtc, AtualizadaEmUtc) VALUES (?, ?, ?)',
        (nome, agora, agora),
    )
    simulacao_id = cursor.lastrowid
    for jogo_id, (gm, gv) in zip(jogos, [(2, 1), (0, 0)]):
        conexao.execute(
            'INSERT INTO "SimulacaoJogos" (SimulacaoId, JogoId, GolsMandante, GolsVisitante) '
            "VALUES (?, ?, ?, ?)",
            (simulacao_id, jogo_id, gm, gv),
        )
    return simulacao_id


def simulacao_exemplo(conexao: sqlite3.Connection) -> int:
    jogos = _dois_jogos(conexao)
    conexao.execute("BEGIN")
    simulacao_id = _criar_simulacao(conexao, "Simulação de exemplo (db-check)", jogos)
    conexao.execute("COMMIT")
    print(f"Simulacao {simulacao_id} criada com 2 SimulacaoJogo (jogos {jogos}).")
    return 0


def restricoes(conexao: sqlite3.Connection) -> int:
    jogos = _dois_jogos(conexao)
    falhas = 0
    conexao.execute("BEGIN")
    try:
        simulacao_id = _criar_simulacao(conexao, "Teste de restrições (revertido)", jogos)

        # 1. Duplicidade (SimulacaoId, JogoId) deve falhar.
        try:
            conexao.execute(
                'INSERT INTO "SimulacaoJogos" (SimulacaoId, JogoId, GolsMandante, GolsVisitante) '
                "VALUES (?, ?, 1, 1)",
                (simulacao_id, jogos[0]),
            )
            print("FALHOU  duplicidade de (SimulacaoId, JogoId) foi aceita")
            falhas += 1
        except sqlite3.IntegrityError as erro:
            print(f"OK      duplicidade rejeitada ({erro})")

        # 2. Excluir Jogo referenciado deve falhar (Restrict).
        try:
            conexao.execute('DELETE FROM "Jogos" WHERE Id = ?', (jogos[0],))
            print("FALHOU  exclusão de Jogo referenciado foi aceita")
            falhas += 1
        except sqlite3.IntegrityError as erro:
            print(f"OK      exclusão de Jogo referenciado rejeitada ({erro})")

        # 3. Excluir Simulacao remove seus SimulacaoJogo (Cascade).
        conexao.execute('DELETE FROM "Simulacoes" WHERE Id = ?', (simulacao_id,))
        (restantes,) = conexao.execute(
            'SELECT COUNT(*) FROM "SimulacaoJogos" WHERE SimulacaoId = ?', (simulacao_id,)
        ).fetchone()
        if restantes == 0:
            print("OK      exclusão da Simulacao removeu seus SimulacaoJogo")
        else:
            print(f"FALHOU  restaram {restantes} SimulacaoJogo após excluir a Simulacao")
            falhas += 1
    finally:
        conexao.execute("ROLLBACK")

    print("Resultado:", "todas as restrições OK" if falhas == 0 else f"{falhas} falha(s)")
    return 1 if falhas else 0


COMANDOS = {
    "contagens": contagens,
    "simulacao-exemplo": simulacao_exemplo,
    "restricoes": restricoes,
}


def main(argv: list[str]) -> int:
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")
    if len(argv) != 3 or argv[2] not in COMANDOS:
        print(__doc__)
        return 2
    conexao = conectar(argv[1])
    try:
        return COMANDOS[argv[2]](conexao)
    finally:
        conexao.close()


if __name__ == "__main__":
    sys.exit(main(sys.argv))
