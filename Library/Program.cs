ILibrary myLibrary = new Library(new LibraryValidation());
myLibrary.AddBook("The Art of Computer Programming", new string[] { "Donald Knuth" }, LibraryCollection.Reserve);
myLibrary.AddBook("Principia Mathematica", new string[] { "Alfred North Whitehead", "Bertrand Russell" }, LibraryCollection.General);
Console.WriteLine("Library list:");
foreach (var book in myLibrary.GetBookNames())
{
    Console.WriteLine($"Title: {book}, Author(s): {String.Join(", ", myLibrary.GetBookAuthors(book))}, Collection: {myLibrary.GetBookCollection(book)}");
}

public enum LibraryCollection
{
    Reserve,
    General
}

public interface ILibrary
{
    void AddBook(string bookName, string[] authors, LibraryCollection collectionToAdd);
    string[] GetBookNames();
    string[] GetBookNames(LibraryCollection collection);
    string[] GetBookAuthors(string bookName);
    LibraryCollection GetBookCollection(string bookName);
}

public class Library : ILibrary
{
    private readonly ILibraryValidation libraryValidation;
    private readonly IDataOperation<Book> dataOperation;

    public Library(ILibraryValidation libraryValidation)
    {
        this.libraryValidation = libraryValidation;
        dataOperation = new LibraryListDataOperation(); // List or HashSet can be used, new data operations like mssql or text can be added.
    }

    public void AddBook(string bookName, string[] authors, LibraryCollection collectionToAdd)
    {
        if (!libraryValidation.IsValidName(bookName) || !libraryValidation.IsValidAuhors(authors))
            throw new ValidationException("Invalid book");

        var book = new Book
        {
            Name = bookName,
            Authors = authors,
            LibraryCollection = collectionToAdd
        };

        var isExist = dataOperation.IsExist(bookName);

        if (isExist)
            dataOperation.Update(book);
        else
            dataOperation.Add(book);
    }

    public string[] GetBookAuthors(string bookName)
    {
        var book = dataOperation.Get(x => x.Name == bookName);

        if (book == null)
            throw new NotFoundException($"{bookName} - Book Not Found!");

        return book.Authors;
    }

    public LibraryCollection GetBookCollection(string bookName)
    {
        return dataOperation.Get(x => x.Name == bookName).LibraryCollection;
    }

    public string[] GetBookNames()
    {
        return dataOperation.GetAll().Select(x => x.Name).ToArray();
    }

    public string[] GetBookNames(LibraryCollection collection)
    {
        return dataOperation.GetList(x => x.LibraryCollection == collection).Select(x => x.Name).ToArray();
    }
}

public interface ILibraryValidation
{
    bool IsValidName(string bookName);

    bool IsValidAuhors(string[] authors);
}

public class LibraryValidation : ILibraryValidation
{
    public bool IsValidAuhors(string[] authors)
    {
        return authors != null && authors.Any();
    }

    public bool IsValidName(string bookName)
    {
        return !string.IsNullOrWhiteSpace(bookName);
    }
}

public interface IDataOperation<T> where T : class, IEntity, new()
{
    void Add(T value);

    void Update(T value);

    bool IsExist(string compareValue);

    List<T> GetAll();

    T Get(Func<T, bool> filter);

    List<T> GetList(Func<T, bool> filter);
}

public class LibraryListDataOperation : IDataOperation<Book>
{
    private List<Book> items = new();

    public void Add(Book value)
    {
        items.Add(value);
    }

    public void Update(Book value)
    {
        var addedItem = items.FirstOrDefault(x => x.Name == value.Name);
        addedItem.Name = value.Name;
        addedItem.Authors = value.Authors;
        addedItem.LibraryCollection = value.LibraryCollection;
    }

    public bool IsExist(string bookName)
    {
        return items.Any(x => x.Name == bookName);
    }

    public List<Book> GetAll()
    {
        return items;
    }

    public Book Get(Func<Book, bool> filter)
    {
        return filter != null ? items.Where(filter).FirstOrDefault() : null;
    }

    public List<Book> GetList(Func<Book, bool> filter)
    {
        return filter != null ? items.Where(filter).ToList() : null;
    }
}

public class LibraryHashSetDataOperation : IDataOperation<Book>
{
    private HashSet<Book> items = new HashSet<Book>();

    public void Add(Book value)
    {
        items.Add(value);
    }

    public void Update(Book value)
    {
        var addedItem = items.FirstOrDefault(x => x.Name == value.Name);
        addedItem.Name = value.Name;
        addedItem.Authors = value.Authors;
        addedItem.LibraryCollection = value.LibraryCollection;
    }

    public bool IsExist(string bookName)
    {
        return items.Any(x => x.Name == bookName);
    }

    public List<Book> GetAll()
    {
        return items.ToList();
    }

    public Book Get(Func<Book, bool> filter)
    {
        return filter != null ? items.Where(filter).FirstOrDefault() : null;
    }

    public List<Book> GetList(Func<Book, bool> filter)
    {
        return filter != null ? items.Where(filter).ToList() : null;
    }
}

public interface IEntity
{

}

public class Book : IEntity
{
    public string Name { get; set; }
    public string[] Authors { get; set; }
    public LibraryCollection LibraryCollection { get; set; }
}


public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}



