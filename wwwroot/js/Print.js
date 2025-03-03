// wwwroot/js/print.js

// Debounce function to delay execution
function debounce(func, wait) {
    let timeout;
    return function (...args) {
        const later = () => {
            clearTimeout(timeout);
            func.apply(this, args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Wait for DOM content to load before attaching events
document.addEventListener("DOMContentLoaded", function () {
    // Attach debounce to search input if it exists
    var searchInput = document.getElementById("searchTerm");
    if (searchInput) {
        searchInput.addEventListener("input", debounce(function () {
            document.getElementById("searchForm").submit();
        }, 500));
    }

    // Attach click event to print button if it exists
    var printButton = document.getElementById("printButton");
    if (printButton) {
        printButton.addEventListener("click", function () {
            // Get the HTML of the hidden print section
            var printContent = document.getElementById("printSection").innerHTML;
            // Open a new window
            var printWindow = window.open("", "_blank", "width=800,height=600");
            printWindow.document.open();
            printWindow.document.write("<html><head><title>Print Bill</title>");
            // Add inline CSS for print styling
            printWindow.document.write("<style>" +
                "body { font-family: 'Arial', sans-serif; margin: 20px; }" +
                ".print-header { text-align: center; margin-bottom: 20px; position: relative; }" +
                ".print-header .site-name { font-size: 28pt; font-weight: bold; margin: 0; }" +
                ".print-header .bill-date { position: absolute; top: 0; right: 0; font-size: 12pt; }" +
                "table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }" +
                "table, th, td { border: 1px solid #333; }" +
                "th, td { padding: 8px; text-align: center; }" +
                "tfoot td { font-weight: bold; }" +
                ".print-footer { text-align: center; margin-top: 30px; font-size: 16pt; font-weight: bold; }" +
                "</style>");
            printWindow.document.write("</head><body>");
            printWindow.document.write(printContent);
            printWindow.document.write("</body></html>");
            printWindow.document.close();
            printWindow.focus();
            printWindow.print();
        });
    }

});
