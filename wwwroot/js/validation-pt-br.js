(function ($) {
    if (!$.validator) return;
    const parseDecimal = value => {
        const text = String(value).trim();
        if (!/^-?(?:\d+(?:[.,]\d+)?|\d{1,3}(?:\.\d{3})+,\d+)$/.test(text)) return NaN;
        return Number(text.includes(',') ? text.replace(/\./g, '').replace(',', '.') : text);
    };
    $.validator.methods.number = function (value, element) {
        return this.optional(element) || Number.isFinite(parseDecimal(value));
    };
    $.validator.methods.range = function (value, element, range) {
        const number = parseDecimal(value);
        return this.optional(element) || (number >= parseDecimal(range[0]) && number <= parseDecimal(range[1]));
    };
    $.extend($.validator.messages, {
        required: 'Preencha este campo.',
        email: 'Informe um e-mail válido.',
        number: 'Informe um número válido. Exemplo: 20,50.',
        digits: 'Use apenas números inteiros.',
        min: $.validator.format('Informe um valor maior ou igual a {0}.'),
        max: $.validator.format('Informe um valor menor ou igual a {0}.')
    });
    // ASP.NET emits an English data-val-number message independently of jQuery defaults.
    $('[data-val-range-min], [data-val-range-max]').each(function () {
        for (const attribute of ['data-val-range-min', 'data-val-range-max']) {
            const raw = this.getAttribute(attribute);
            if (raw !== null) this.setAttribute(attribute, String(parseDecimal(raw)));
        }
    });
    $('[data-val-number]').each(function () {
        $(this).attr('data-val-number', 'Informe um número válido. Exemplo: 20,50.');
    });
})(jQuery);
