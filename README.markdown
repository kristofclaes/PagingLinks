# PagingLinks #
This is my small HtmlHelper extension method to generate paging links in ASP.NET MVC.

## Usage ##
    @Html.PagingLinks(Model.CurrentPage, Model.TotalPages)
	
or

    @Html.PagingLinks(Model.CurrentPage, Model.TotalPages, "p")

The third optional parameter specifies the name of the routevalue or querystring parameter that contains the pagenumber.