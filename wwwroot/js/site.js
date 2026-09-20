// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.table-responsive').forEach((container, index) => {
        const hint = document.createElement('p');
        hint.id = `table-scroll-hint-${index}`;
        hint.className = 'table-scroll-hint';
        hint.textContent = 'Deslize a tabela para ver todas as colunas e ações. No teclado, use as setas.';
        container.before(hint);
        const updateOverflow = () => {
            const overflowing = container.scrollWidth > container.clientWidth + 1;
            hint.hidden = !overflowing;
            if (overflowing) {
                container.setAttribute('tabindex', '0');
                container.setAttribute('role', 'region');
                container.setAttribute('aria-label', 'Tabela com rolagem horizontal');
                container.setAttribute('aria-describedby', hint.id);
            } else {
                ['tabindex', 'role', 'aria-label', 'aria-describedby'].forEach(name => container.removeAttribute(name));
            }
        };
        const observer = new ResizeObserver(updateOverflow);
        observer.observe(container);
        const table = container.querySelector('table');
        if (table) observer.observe(table);
        updateOverflow();
    });
    const normalize = value => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('pt-BR');
    document.querySelectorAll('table[data-search-label]').forEach((table, index) => {
        const rows = [...table.querySelectorAll('tbody tr')].filter(row => !row.querySelector('[colspan]'));
        if (!rows.length) return;
        const wrapper = document.createElement('div');
        wrapper.className = 'table-search';
        const label = document.createElement('label');
        const input = document.createElement('input');
        input.id = `table-search-${index}`;
        input.type = 'search';
        input.className = 'form-control';
        input.placeholder = table.dataset.searchLabel;
        label.htmlFor = input.id;
        label.textContent = table.dataset.searchLabel;
        const feedback = document.createElement('p');
        feedback.setAttribute('role', 'status');
        wrapper.append(label, input, feedback);
        table.closest('.table-responsive').before(wrapper);
        input.addEventListener('input', () => {
            const query = normalize(input.value.trim());
            let count = 0;
            rows.forEach(row => {
                const searchable = [...row.cells].filter(cell => !cell.querySelector('form,button,.btn')).map(cell => cell.textContent).join(' ');
                row.hidden = !normalize(searchable).includes(query);
                if (!row.hidden) count++;
            });
            feedback.textContent = query ? (count ? `${count} resultado(s) encontrado(s).` : 'Nenhum resultado. Tente outro nome ou limpe a busca.') : '';
        });
    });
});
