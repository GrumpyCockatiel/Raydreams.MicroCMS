# MicroCMS

A rediculously simple CMS that serves Markdown content files in either an Azure File Share or Blob Container.

No web server needed... no database is used.

This runs as a .NET 6 isolated Azure Function, which means it has a main function that you can inject into at startup just like a web API.

It uses the [MarkDig](https://github.com/xoofx/markdig) Markdown to HTML converter but you can easily uses any converter you like. All the Advanced Parsing options are turned on so you can add IDs, Classes and even custom attributes to the HTML from the Markdown. This allows you to tweak the layout from the CSS while still using Markdown.

## Example

[My Test Blog](https://blog.raydreams.com/page/index)

## The Repo

[MicroCMS on GitHub](https://github.com/GrumpyCockatiel/Raydreams.MicroCMS)

## Use Case

I really just wanted a simple way to post Markdown files without having to use someone else's service. Even [Jekyll](https://jekyllrb.com/) was overkill. Seriously if you think Jekyll is "simple" you are smokin' something. I mean it's cool and all but...

Markdown is much easier and faster to type without all the ridiculous HTML. You can make Markdown as basic or advanced as you like.

## Layouts

The concept is pretty simple... Make any HTML template and put it in a templates folder like `main.html`.

For the body sections just use something like:
```
<div class="container">
    <div>{% BODY %}</div>
</div>
```

For now the token must be exactly `{% BODY %}`. I'm adding more tokens but the only other one for now is `{% TIMESTAMP %}`.

So browsing to the homepage would be
```
https://www.myblog.com/page/index?layout=<layout>
```

For exmaple:
[Dark Mode Layout](https://blog.raydreams.com/page/index?layout=dark)

## Front Matter

Front matter lets you add Metadata to a Markdown file. As of right now Front Matter is supported and parsed but it's not yet implemented. It's just a YML header at the start of the file like:

```
---
title: My Title
---

# Welcome to the New World
```

## Endpoints

For now there are only a couple of endpoints :

* **GetHome** - Intercepts any call to the root for now and redirects back to the home page
  * path: [GET] /
* **GetPage** - Gets a single Markdown page from the data store and returns it as HTML or wrapped JSON
  * path: [GET] /page/<filename_with_no_ext>?layout=&lt;file&gt;&wrapped=&lt;bool&gt;
  * params:
    * layout (string) [optional] to use an explicit layout instead of the default one
    * wrapped (bool) [optional] set to true to return the page as wrapped JSON for using as headless
  * output: Either the HTML or a wrapped JSON object
* **GetImage** - Gets a single image of type JPEG, PNG, GIF, or ICO
  * path: [GET] /image/<filename_with_ext>
  * params: none
  * output: an image file
* **Ping** - Just test the service is working
  * path: [GET] /ping/<some_string_message_to_echo>
  * params: none
  * output: a JSON string
* **List** - Get an HTML list of all the pages. An integer of some value is required but has no effect just yet
  * path: [GET] /list/&lt;integer&gt;
  * params: none
  * output: HTML list of files

## Getting Started

(More Details Incoming)

* Create an Azure Account with a Free Pay-As-You-Go subscription.
* Create a new Azure Function Resource. You can use a Windows or Liunx App Service.
    * You can use the Data Storage account created with it but I suggest giving it a better name like `blogdevsa01`
* Fork this repo and publish the main function app project to our new Azure Function
    * You can publish from Visual Studio or just accept the default YML script from Deployment Center
    * Make sure you are using Azure Function runtime v4 (this is NOT the same as the .NET version)
    * Make sure you set `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated` in Configuration.
* Upload some Markdown files, a base layout file and some images to the corresponding Data Store that was created with the Azure Function.
* Add a Single table called `Logs` to your Azure Storage Tables.

## Configure the function settings

There are handful of settings you will want to set either in `local.settings.json` or the Azure Function Configuration:

```
"Values": {
    "AzureWebJobsStorage": "<Already set in Azure Function>",
    "ASPNETCORE_ENVIRONMENT": "Development",
    "connStr": "Connection String to the Data Storage You want to use",
    "root": "blog",
    "homepage": "index",
    "errorpage": "error",
    "imageDir": "image",
    "layoutDir": "layouts",
    "layoutExt": "html",
    "store": "file"
}
```

![Azure Function Configuration](./readme/config-1.png)

## Local Folder Watcher

There's a second project called `Raydreams.MicroCMS.CLI` which is an exe that will watch a specific folder on your machine.

You must add the `appsettings.json` file to your project with similar data as local.settings.json

```
"AppConfig": {
    "LocalRoot": "",
    "RemoteRoot": "blog",
    "ConnectionString": "<Data_Store_Connection_String>",
    "LocalImagesDir": "image",
    "LocalLayoutsDir": "layouts",
    "LocalPagesDir": "page"
}
```

You can run the exe on OS X using:
```
dotnet MicroCMS.dll -w <full_path_to_local_blog_root>
dotnet MicroCMS.dll -w "/Users/bob/Desktop/MyBlog/"
```

On Windows of course you can run just the exe natively itself.

On a file change or addition, the file will be updated remotely. On file deletion or removal, the file will be removed remotely.

## Local Structure

To make it easier to preview you Markdown with images you can create a root folder named anything you like

Local Folder Structure:
+ MyBlog
  + image
  + layouts
  + page
    + subfolder1
    + subfolder2

MicroCMS will only look for images in the image folder. I'm working on this to make it more customizable.

## Headless Too

It's even headless. Just addd `wrapped=true` to the URL.

```
https://blog.raydreams.com/page/index?wrapped=true
```

The resulting outer JSON object is not finalized.

## Testing

No unit test yet just simple integration test. More to come.

