### EshopAPI

A REST API for a music shop, built with ASP.NET Core and PostgreSQL. It manages a product catalogue and customer orders, including stock checks and order totals.

This is a portfolio project. It covers the backend of the shop, from saving products to placing an order and returning useful error messages to the frontend.

#### What it does

Products have a title, artist, description, price, stock quantity, image URL and category.

Customers can place an order with their name, email, shipping address and a list of products. The API checks that each product exists and that enough stock is available. It uses prices from the database to calculate the total, then saves the order and reduces the stock. Each order item keeps the price from the time of purchase.

#### Built with

- C# and .NET 10
- ASP.NET Core controllers
- Entity Framework Core
- PostgreSQL
- OpenAPI and Scalar for exploring the endpoints

#### Running it locally

From the project directory, restore the packages:

```bash
dotnet restore
```

Store your database URL in user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://USER:PASSWORD@HOST:5432/DATABASE"
```

Replace the placeholders with your own connection details. If your username or password contains special characters, URL-encode them. The application expects a PostgreSQL URL, rather than a connection string starting with `Host=`.

For deployment, use an environment variable named `ConnectionStrings__DefaultConnection`. Keep real credentials out of the repository.

Install the EF Core command-line tool if you do not already have a compatible version:

```bash
dotnet tool install --global dotnet-ef --version "10.*"
```

Apply the migrations and start the API:

```bash
dotnet ef database update -- --environment Development
dotnet run --launch-profile http
```

The API runs at `http://localhost:5052`. Open `http://localhost:5052/scalar/v1` to try the endpoints. Scalar and the OpenAPI document are available in the Development environment.

#### Endpoints

| Method | Route | What it does |
| --- | --- | --- |
| GET | `/api/products` | Lists all products |
| GET | `/api/products/{id}` | Returns one product |
| GET | `/api/products/category/{category}` | Lists products in a category |
| POST | `/api/products` | Creates a product |
| PUT | `/api/products/{id}` | Updates a product |
| DELETE | `/api/products/{id}` | Deletes a product |
| GET | `/api/orders` | Lists orders with their items |
| GET | `/api/orders/{id}` | Returns one order with its items |
| POST | `/api/orders` | Creates an order and reduces stock |
| DELETE | `/api/orders/{id}` | Deletes an order |

`PUT` expects the full set of editable product fields.

For example, a product request looks like this:

```json
{
  "title": "Kind of Blue",
  "artist": "Miles Davis",
  "description": "A jazz album released in 1959.",
  "price": 24.90,
  "stock": 10,
  "imageUrl": "https://example.com/kind-of-blue.jpg",
  "productCategory": "Jazz"
}
```

An order request looks like this. Use an existing product ID:

```json
{
  "customerName": "Alex Smith",
  "email": "alex@example.com",
  "shippingAddress": "12 Example Street, Athens",
  "items": [
    {
      "productId": 1,
      "quantity": 2
    }
  ]
}
```

The client does not need to send prices or a total. The API calculates them.

#### Responses and errors

Successful reads and product updates return `200`. Creating a product or an order returns `201` with the saved data and a `Location` header. Successful deletions return `204` with no body. A list with no results returns `200` and `[]`.

A missing product or order returns `404` with a short message such as `Product not found.`. An order without enough stock returns `400` with a message explaining which product is unavailable.

DTO validation also returns `400`, with a JSON body containing an `errors` object. These checks cover required fields, email and image URL formats, product field lengths, price limits, non-negative stock and positive order quantities. An order must contain at least one item.

Unexpected exceptions are logged in each controller action and return `500` with a simple English message. Exception details stay in the application logs. In the frontend, read the plain text body for the controller messages and the JSON body for validation errors.

#### Checking the API

The repository includes a Python script that sends real HTTP requests. It uses only the Python 3 standard library.

The latest local run passed 86 checks against PostgreSQL, including the error responses when the database was unavailable. The [test results](docs/api-test-results.md) describe what was checked and the validation fix found during the run.

Use a separate, empty PostgreSQL database for these tests. In a separate terminal, set its URL, apply the migrations and start the API:

```bash
export ConnectionStrings__DefaultConnection='postgresql://USER:PASSWORD@HOST:5432/eshop_api_test'
dotnet ef database update -- --environment Development
dotnet run --launch-profile http
```

Then run the checks from another terminal:

```bash
python3 scripts/smoke_test.py --base-url http://localhost:5052
```

The script checks product and order endpoints, validation errors, missing records, order totals, saved prices and stock changes. It also checks that rejected orders leave stock unchanged. It refuses to create data if either collection is already populated and removes its own records when it finishes.

To check the error responses when the database is unavailable, start another API instance with an unreachable local database address:

```bash
ConnectionStrings__DefaultConnection='postgresql://test:test@127.0.0.1:1/eshop_api_test' \
  dotnet run --no-launch-profile -- --urls http://localhost:5053
```

Run the error checks against that instance:

```bash
python3 scripts/smoke_test.py --base-url http://localhost:5053 --database-unavailable
```

#### Current limits / future improvements

Authentication and authorization have not been added yet. Product changes and access to customer orders are currently unrestricted, so this version should only be used with demo data.

Stock checks do not yet protect against simultaneous orders buying the same last item. Product deletion also needs attention: the current migrations use cascade delete for order items linked to a product, which can affect order history. Deleting an order does not restore stock.

Payments, delivery tracking and a customer account system are outside the current scope.
