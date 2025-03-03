// Pre-built options for the Food Item dropdown (used for dynamic rows)
// Get food options from the data attribute
var foodOptions = $('#billItemInputs').data('options-html');

// Calculate a single row's total based on price, quantity, and weight type.
function calculateRowTotal(row) {
    var price = parseFloat($(row).find('.price').val()) || 0;
    var quantity = parseFloat($(row).find('.quantity').val()) || 0;
    var weightType = $(row).find('.weightType').val();
    var total = 0;
    if (weightType === 'Grams') {
        // If price is per Kg, convert quantity (grams to Kg)
        total = (quantity / 1000) * price;
    } else {
        total = price * quantity;
    }
    $(row).find('.total').val(total.toFixed(2));
}

// Calculate grand total from all rows.
function calculateGrandTotal() {
    var grandTotal = 0;
    $('.total').each(function () {
        grandTotal += parseFloat($(this).val()) || 0;
    });
    $('#grandTotal').val(grandTotal.toFixed(2));
}

// Validate if the current row is completely filled before adding a new row
function isCurrentRowFilled() {
    var lastRow = $('.bill-item-row').last();
    var foodItem = lastRow.find('select[name*="FoodItemId"]').val();
    var price = lastRow.find('.price').val();
    var quantity = lastRow.find('.quantity').val();
    var weightType = lastRow.find('.weightType').val();

    return foodItem && price && quantity && weightType;
}

$(document).ready(function () {
    $(document).on('input change', '.price, .quantity, .weightType', function () {
        var row = $(this).closest('.bill-item-row');
        calculateRowTotal(row);
        calculateGrandTotal();
    });
    $('.bill-item-row').each(function () {
        calculateRowTotal(this);
    });
    calculateGrandTotal();
});

// Add a new row dynamically, only if the current row is filled
//function addBillItemRow() {
//    if (!isCurrentRowFilled()) {
//        toastr.error('Please fill in all fields in the current row before adding a new row.');
//        return;
//    }

//    var index = $('.bill-item-row').length;
//    var rowHtml = `
//        <div class="row g-2 mb-2 bill-item-row">
//            <input type="hidden" name="BillDetails[` + index + `].BillId" value="@Model.BillId" />
//            <div class="col-md-2">
//                <select class="form-select shadow-sm" name="BillDetails[` + index + `].FoodItemId" required>
//                    <option value="">Select Food Item</option>
//                    ` + foodOptions + `
//                </select>
//            </div>
//            <div class="col-md-2">
//                <input type="number" step="0.01" class="form-control price shadow-sm" name="BillDetails[` + index + `].Price" placeholder="Price" value="0" required />
//            </div>
//            <div class="col-md-2">
//                <input type="number" step="0.01" class="form-control quantity shadow-sm" name="BillDetails[` + index + `].Quantity" placeholder="Quantity" value="0" required />
//            </div>
//            <div class="col-md-2">
//                <select class="form-select weightType shadow-sm" name="BillDetails[` + index + `].WeightType" required>
//                    <option value="Grams">Grams</option>
//                    <option value="Kg">Kg</option>
//                    <option value="Pieces">Pieces</option>
//                </select>
//            </div>
//            <div class="col-md-2">
//                <input type="number" step="0.01" readonly class="form-control total shadow-sm" name="BillDetails[` + index + `].Total" placeholder="Total" value="0" />
//            </div>
//            <div class="col-md-2">
//                <button type="button" class="btn btn-danger shadow-sm" onclick="removeBillItemRow(this)">
//                    <i class="bi bi-trash"></i>
//                </button>
//            </div>
//        </div>`;

//    $('#billItemInputs').append(rowHtml);
//}

//// Remove a row and recalculate the grand total.
//function removeBillItemRow(button) {
//    $(button).closest('.bill-item-row').remove();
//    calculateGrandTotal();
//}
