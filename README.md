- [AspNet Core jQuery Datatable](#aspnet-core-jquery-datatable)
  - [MSSQL Database](#mssql-database)
  - [API Reference](#api-reference)
      - [Get basic datatables request](#get-basic-datatables-request)
      - [Post Pagination](#post-pagination)
      - [Post Searching](#post-searching)
      - [Post Searching](#post-searching-1)
    - [Request Form Control from jQuery Datatable Ajax Server side Process](#request-form-control-from-jquery-datatable-ajax-server-side-process)
    - [jQuery Ajax Server Side Ajax Request](#jquery-ajax-server-side-ajax-request)
        - [This script is only work for pagination and searching but data ordering doesn't work propably.](#this-script-is-only-work-for-pagination-and-searching-but-data-ordering-doesnt-work-propably)
        - [So we need to add the column name in this script.](#so-we-need-to-add-the-column-name-in-this-script)
        - [Customizing UI with dom of datatable option](#customizing-ui-with-dom-of-datatable-option)


---

https://github.com/carloswm85/AspNetCoreDatatable

# AspNet Core jQuery Datatable

 Datatable Pagination with API
 This is the jQuery Datatable Custom UI and Custom Mode. Including pagination, searching and ordering.
 Querying the 13,000 rows from MSSQL Server with linq query.

 ![image](https://user-images.githubusercontent.com/57518163/220984421-6d5705a2-c0aa-41ca-abd4-846f0f0f0fc8.png)

## MSSQL Database
Download and restore the database backup file. Named as AspNetCore_Datatable [Download here](https://github.com/aungaung99/AspNetCoreDatatable/blob/main/aspnetcore_datatable.bak)

## API Reference

#### Get basic datatables request
```
  [GET] /api/datatables
```

#### Post Pagination

```
  [POST] /api/datatables/pagination
```
Datatables draw page by linq query skipping and taking method

#### Post Searching

```
  [POST] /api/datatables/searching
```
Datatables draw search value by linq query contain method

#### Post Searching

```
  [POST] /api/datatables/ordering
```
Datatables sort / order by linq query orderby & orderbydescending method

### Request Form Control from jQuery Datatable Ajax Server side Process

```cs
[HttpPost]
public async Task<IActionResult> OnPostAsync()
{
   try
   {
       // Create a config object
       ParsingConfig config = new ParsingConfig
       {
           UseParameterizedNamesInDynamicQuery = true
       };

       string draw = Request.Form["draw"].FirstOrDefault();
       string start = Request.Form["start"].FirstOrDefault();
       string length = Request.Form["length"].FirstOrDefault();
       string sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
       string sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
       string searchValue = Request.Form["search[value]"].FirstOrDefault();
       int pageSize = length != null ? Convert.ToInt32(length) : 0;
       int skip = start != null ? Convert.ToInt32(start) : 0;
       int recordsTotal = 0;

       IQueryable<UserInfo> userInfo = (from dbUserInfo in _context.UserInfos select dbUserInfo);
       recordsTotal = userInfo.Count();

       if (!string.IsNullOrEmpty(searchValue))
       {
           userInfo = userInfo.Where(m => m.Name.Contains(searchValue)
                                       || m.Gender.Contains(searchValue)
                                       || m.EyeColor.Contains(searchValue)
                                       || m.Email.Contains(searchValue)
                                       || m.Phone.Contains(searchValue)
                                       || m.Company.Contains(searchValue));
       }

       if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
       {
           userInfo = userInfo.OrderBy(sortColumn + " " + sortColumnDirection);
       }

       List<UserInfo> data = pageSize < 0 ? await userInfo.ToListAsync() : await userInfo.Skip(skip).Take(pageSize).ToListAsync();
       var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
       return Ok(jsonData);

   }
   catch (Exception)
   {
       return BadRequest();
       throw;
   }
}
```

### jQuery Ajax Server Side Ajax Request
##### This script is only work for pagination and searching but data ordering doesn't work propably.

```javascript
  $('#example').DataTable({
       ajax: {
           url: '/api/...',
           type: 'POST',
           dataType: 'JSON',
       },
       processing: true,
       serverSide: true,
       columns: [
           { data: 'userId' },
           { data: 'name' }
       ]
});
```
![image](https://user-images.githubusercontent.com/57518163/220983553-e4f57825-b860-4653-b34f-823fdfbb5141.png)

##### So we need to add the column name in this script.

```javascript
  $('#example').DataTable({
       ajax: {
           url: '/api/...',
           type: 'POST',
           dataType: 'JSON',
       },
       processing: true,
       serverSide: true,
       columns: [
           { name: 'UserId', data: 'userId' },
           { name: 'Name', data: 'name' }
       ]
});
```

![image](https://user-images.githubusercontent.com/57518163/220998049-f5cb3ac5-ef9d-4414-8d5e-22dc942f3d4b.png)

##### Customizing UI with dom of datatable option

```javascript
  $('#example').DataTable({
       ajax: {
           url: '/api/...',
           type: 'POST',
           dataType: 'JSON',
       },
       processing: true,
       serverSide: true,
       dom: "<'row'<'col-sm-12'tr>>" +
                "<'row d-flex justify-content-between'<'col-auto'p><'col-auto float-end mt-2'l>>",
       columns: [
           { name: 'UserId', data: 'userId' },
           { name: 'Name', data: 'name' }
       ]
});
```

![image](https://user-images.githubusercontent.com/57518163/220998948-91dc10b3-1703-4912-a929-5eaa4ffdb41f.png)

Response JSON Value
```json
{  
  "data" : 
  [
     { 
       "userId" : "001",
       "name"   : "Mg Mg",
     },
      { 
       "userId" : "002",
       "name"   : "Ag Ag",
     }
   ]
}
```




