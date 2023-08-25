## StaticFileSecureCall
###### Secure static file upload and download made possible.
###### NB : Elastic Beanstalk sets the ASPNETCORE_ENVIRONMENT environment variable to Production when your application is deployed to a production environment.
###### We have added LinqKit.EntityFramework to enhance functionalities of entity framework Queries.
###### NB: The Dbset should never be referenced from the namespace System.Data.Entity as this may lead to Error "Only sources that implement IDbAsyncEnumerable can be used for Entity Framework asynchronous operations"
###### Generic DBSets&lt;TEntity&gt; should only reference Microsoft.EntityFrameworkCore namespace.
###### Autofac may be introduced if dependencies become too many for cleaner code and maintainability
###### Code Architecture heavilty utilises Domain Driven Design (D.D.D.) and dependency injection(D.I.).
###### Dependency Injection has been heavily utilized to maintain abstraction from Service templates and their respective implementations.
