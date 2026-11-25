// ============================================================
// PortalCopa26 — integração com Chart.js (módulo ES, usado via JSInterop)
// Usa o objeto global Chart carregado pelo App.razor.
//
// Convenções declarativas aceitas em config.options (callbacks JS não
// são serializáveis a partir do .NET):
//   formatoNumero: "pt-BR"  → formata a dica e os ticks do eixo de
//                              valores com toLocaleString(locale).
//   sufixoValor: "pts"      → sufixo exibido após o valor na dica.
// ============================================================

const graficos = new WeakMap();

function eixoDeValores(config) {
    return config.options?.indexAxis === 'y' ? 'x' : 'y';
}

function aplicarConvencoes(config) {
    const options = config.options ?? (config.options = {});
    const locale = options.formatoNumero;
    const sufixo = options.sufixoValor ? ` ${options.sufixoValor}` : '';
    delete options.formatoNumero;
    delete options.sufixoValor;

    if (!locale) {
        return config;
    }

    const eixo = eixoDeValores(config);
    const formatar = valor => Number(valor).toLocaleString(locale);

    options.plugins ??= {};
    options.plugins.tooltip ??= {};
    options.plugins.tooltip.callbacks ??= {};
    options.plugins.tooltip.callbacks.label = contexto =>
        ` ${formatar(contexto.parsed[eixo])}${sufixo}`;

    options.scales ??= {};
    options.scales[eixo] ??= {};
    options.scales[eixo].ticks ??= {};
    options.scales[eixo].ticks.callback = valor => formatar(valor);

    return config;
}

/** Cria (ou recria) o gráfico no canvas informado. */
export function criarGrafico(canvas, config) {
    destruirGrafico(canvas);
    graficos.set(canvas, new Chart(canvas, aplicarConvencoes(config)));
}

/** Substitui dados e opções do gráfico existente; cria o gráfico se ainda não existir. */
export function atualizarGrafico(canvas, config) {
    const grafico = graficos.get(canvas);
    if (!grafico || grafico.config.type !== config.type) {
        criarGrafico(canvas, config);
        return;
    }

    const convertido = aplicarConvencoes(config);
    grafico.data = convertido.data;
    grafico.options = convertido.options ?? {};
    grafico.update();
}

/** Destrói o gráfico associado ao canvas, se houver. */
export function destruirGrafico(canvas) {
    const grafico = graficos.get(canvas);
    if (grafico) {
        grafico.destroy();
        graficos.delete(canvas);
    }
}
