# Unicode Browser

A [Blazor](https://blazor.net/) C# web application for browsing the Unicode character database.

The application is hosted there: https://unicode-browser.azurewebsites.net/home

## Features

* Decompose text in Unicode code points.
* View detailed information on a code point.
* Browse code points in a given Unicode Block.
* Search any character by name.
* Display additional information (in Markdown format) on pages dedicated to Unicode blocks.

## Try it out

 Here are a few URLs that you can try, to see the application in action:

* https://unicode-browser.azurewebsites.net/blocks/Miscellaneous%20Symbols%20and%20Pictographs
* https://unicode-browser.azurewebsites.net/codepoints/1F365
* https://unicode-browser.azurewebsites.net/search?q=Tree
* https://unicode-browser.azurewebsites.net/decomposition?text=👨🏿‍👩🏻‍👧🏽‍👦🏽
* https://unicode-browser.azurewebsites.net/blocks/Tangut
* https://unicode-browser.azurewebsites.net/blocks/Tags

## Main technologies used

* [ASP.NET Core](https://dot.net/) 3.0 (.NET Core 3.0.0-preview8.19405.7)
* [Blazor](https://blazor.net/)
* [Markdig](https://github.com/lunet-io/markdig) 0.17.1
* [Bootstrap](https://getbootstrap.com/) 4.1
* [Font Awesome](https://fontawesome.com/) 5.0.10
* [jQuery](https://jquery.com/) 3.3.1 (Beacuse Bootstrap JS still seems to require it)

## Documentation links

Blazor documentation is available at [docs.microsoft.com](https://docs.microsoft.com/aspnet/core/blazor).

## Backstory

Most of this application was originally written back in early 2016, using a preview version of ASP.NET Core, TypeScript and [Aurelia](http://aurelia.io/).
At the time, this was an occasion to learn about those very new frameworks, and it worked great.

The source code of the current version is mostly a straightforward adaptation of the early prototype.
In fact, most of the server code was simply ported to ASP.NET Core 2.1 with very little modifications.

The client part, however, was where the "fun" was, as only the HTML parts were reusable. (And still required a migration to Bootstrap 4 / Fontawesome 5)
This, again, was a great occasion to test and learn how to use Blazor, as weel as keeping up-to-date with the other technologies.

## Gotchas

Blazor is a very new technology, still in early preview, and can be hard to deal with at times.

Because of that, I found myself stumbling on things that I thought would be trivial, and I feel that I had to write a lot of boilerplate code for getting these things to work.

e.g

~~* I was quite disappointed that I could not share my Model classes between the Server and Client, as Blazor currently uses SimpleJson, which is hugely limited. 😅
  (And sadly, Newtonsoft.Json doen't seem to work well enough for now. 😭)~~
* Binding to query string parameters has to be done manually
* Pages sometimes need to manually listen for location changes (e.g. for URL changes on the same page)
* There is no state management out of the box. (e.g. What happens when you go back and forward in the history)
  => I solved that by not requiring state management at all, and always relying on the route or query string for that.
* The component lifecycle did not seem to be very extensible: it would be difficult to add a complex behavior in a base class (e.g. displaying a spinner before the content is loaded)

Things have improved in 3.0 ([route parameters are now decoded](https://github.com/aspnet/AspNetCore/pull/8759) 😙) but I think most of the points still remain true.

A few novelties for .NET Core 3.0:

* System.Text.Json is the new (default) JSON serializer for Blazor. However, it is very rough on the edges. Unavailablity to deserialize immutable types out of the box is a real pain. 😕
  => I implemented a JsonConverter for managing this specific case. By relying on dynamic code generation, one can create a type that the new API will accept to deserialize, then instanciate the immutable type.
* In Preview 8, Blazor client projects still have to target .NET Standard 2.0 and not .NET Standard 2.1.
