// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Shared trigger for the reusable "_DeleteConfirmModal" partial. Pass the clicked button
// (with data-name/data-delete-url attributes) and, optionally, the modalId used when the
// partial was rendered (defaults to "deleteConfirmModal").
function confirmDelete(button, modalId) {
    modalId = modalId || 'deleteConfirmModal';
    document.getElementById(modalId + 'Name').textContent = button.getAttribute('data-name');
    document.getElementById(modalId + 'Form').action = button.getAttribute('data-delete-url');
    bootstrap.Modal.getOrCreateInstance(document.getElementById(modalId)).show();
}

// Initializes bootstrap-select on every ".selectpicker" element on the page.
function initSelectpickers() {
    if (window.jQuery && $.fn.selectpicker) {
        $('.selectpicker').selectpicker();
    }
}

// Initializes flatpickr on every element matching inputSelector, wiring up the calendar
// icon (".received-date-toggle" within the input's closest ".received-date-group") to open
// the picker. Extra flatpickr options (e.g. onChange) can be passed via the options param.
function initDateGroupPickers(inputSelector, options) {
    if (!window.flatpickr) return [];
    var pickers = [];
    document.querySelectorAll(inputSelector).forEach(function (input) {
        var picker = flatpickr(input, Object.assign({ dateFormat: 'Y-m-d', allowInput: true, appendTo: document.body }, options || {}));
        var toggle = input.closest('.received-date-group')?.querySelector('.received-date-toggle');
        if (toggle) toggle.addEventListener('click', function () { picker.open(); });
        pickers.push(picker);
    });
    return pickers;
}
