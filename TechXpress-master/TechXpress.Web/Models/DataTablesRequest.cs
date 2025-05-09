namespace TechXpress.Web.Models
{
    // TechXpress.Web/Models/DataTablesRequest.cs
    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public DataTablesColumn[] Columns { get; set; }
        public DataTablesOrder[] Order { get; set; }
        public DataTablesSearch Search { get; set; }
    }

    public class DataTablesColumn
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public DataTablesSearch Search { get; set; }
    }

    public class DataTablesOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

    public class DataTablesSearch
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class DataTablesResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public object Data { get; set; }

        public DataTablesResponse(int draw, object data, int recordsTotal, int recordsFiltered)
        {
            Draw = draw;
            Data = data;
            RecordsTotal = recordsTotal;
            RecordsFiltered = recordsFiltered;
        }
    }

    // TechXpress.Web/Models/PaginatedList.cs
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

    // TechXpress.Web/Extensions/StringExtensions.cs
    public static class StringExtensions
    {
        public static string EscapeCsvField(this string field)
        {
            if (field == null) return string.Empty;
            return field.Contains('"') ? field.Replace("\"", "\"\"") : field;
        }

        public static string SanitizeFileName(this string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(fileName
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray())
                .Replace(" ", "_");
        }
    }
}
