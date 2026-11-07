using System.Globalization;
using System.Net;
using System.Text;
using JamunaERP.Application.Common;
using JamunaERP.Application.Sales.DTOs;

namespace JamunaERP.WebApi.Printing;

public static class InvoiceHtmlBuilder
{
    private const string Css =
        "body{font-family:Arial,sans-serif;max-width:800px;margin:20px auto;color:#222}" +
        "h1{margin:0}table{width:100%;border-collapse:collapse;margin:12px 0}" +
        "th,td{border-bottom:1px solid #ddd;padding:6px;text-align:left}" +
        "th.n,td.n{text-align:right}.muted{color:#666;font-size:13px}" +
        "@media print{button{display:none}}";

    private static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);
    private static string M(decimal v) => "৳" + v.ToString("N2", CultureInfo.InvariantCulture);

    public static string Build(InvoiceDto inv, string shopName, string? shopAddress, string? shopPhone)
    {
        var sb = new StringBuilder();
        var date = ShopTime.ToLocal(inv.SaleDate).ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

        sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Invoice ")
          .Append(E(inv.InvoiceNo)).Append("</title><style>").Append(Css).Append("</style></head><body>");

        sb.Append("<h1>").Append(E(shopName)).Append("</h1>");
        sb.Append("<div class=\"muted\">").Append(E(shopAddress)).Append(' ').Append(E(shopPhone)).Append("</div><hr>");

        sb.Append("<p><b>Invoice:</b> ").Append(E(inv.InvoiceNo))
          .Append("<br><b>Date:</b> ").Append(date)
          .Append("<br><b>Customer:</b> ").Append(E(inv.CustomerName))
          .Append(" &middot; ").Append(E(inv.CustomerPhone))
          .Append("<br>").Append(E(inv.CustomerAddress)).Append("</p>");

        sb.Append("<table><tr><th>Item</th><th class=\"n\">Qty</th><th class=\"n\">Price</th><th class=\"n\">Total</th></tr>");
        foreach (var l in inv.Items)
        {
            sb.Append("<tr><td>").Append(E(l.ProductName)).Append(" (").Append(E(l.ModelNo)).Append(')');
            if (l.WarrantyMonths > 0)
                sb.Append("<div class=\"muted\">Warranty: ").Append(l.WarrantyMonths).Append(" months</div>");
            sb.Append("</td><td class=\"n\">").Append(l.Quantity)
              .Append("</td><td class=\"n\">").Append(M(l.UnitPrice))
              .Append("</td><td class=\"n\">").Append(M(l.LineTotal)).Append("</td></tr>");
        }
        sb.Append("</table>");

        sb.Append("<table style=\"width:50%;margin-left:auto\">");
        void Row(string label, decimal value) =>
            sb.Append("<tr><td>").Append(label).Append("</td><td class=\"n\">").Append(M(value)).Append("</td></tr>");

        Row("Subtotal", inv.SubTotal);
        if (inv.DiscountAmount > 0) Row("Discount", -inv.DiscountAmount);
        Row("<b>Total</b>", inv.TotalAmount);
        if (inv.ReturnedAmount > 0) Row("Returned", -inv.ReturnedAmount);
        Row("Paid", inv.PaidAmount);
        if (inv.RefundedAmount > 0) Row("Refunded", -inv.RefundedAmount);
        Row("<b>Balance due</b>", inv.BalanceDue);
        sb.Append("</table>");

        if (inv.Installments.Count > 0)
        {
            sb.Append("<h3>Installments</h3><table><tr><th>#</th><th>Due date</th><th class=\"n\">Amount</th><th class=\"n\">Paid</th><th>Status</th></tr>");
            foreach (var i in inv.Installments)
            {
                sb.Append("<tr><td>").Append(i.Number).Append("</td><td>")
                  .Append(i.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture))
                  .Append("</td><td class=\"n\">").Append(M(i.AmountDue))
                  .Append("</td><td class=\"n\">").Append(M(i.AmountPaid))
                  .Append("</td><td>").Append(E(i.Status)).Append("</td></tr>");
            }
            sb.Append("</table>");
        }

        if (inv.Payments.Count > 0)
        {
            sb.Append("<h3>Payments</h3><table><tr><th>Date</th><th>Method</th><th>Receipt</th><th class=\"n\">Amount</th></tr>");
            foreach (var p in inv.Payments)
            {
                sb.Append("<tr><td>")
                  .Append(ShopTime.ToLocal(p.Date).ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture))
                  .Append("</td><td>").Append(E(p.Method))
                  .Append("</td><td>").Append(E(p.ReceiptNo))
                  .Append("</td><td class=\"n\">").Append(M(p.Amount)).Append("</td></tr>");
            }
            sb.Append("</table>");
        }

        sb.Append("<p class=\"muted\">Thank you for shopping with us.</p>");
        sb.Append("<button onclick=\"window.print()\">Print</button></body></html>");
        return sb.ToString();
    }
}