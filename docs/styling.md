# Styling

This project is configured to use scss for styling. All scss files will be compiled at build time. CSS files are excluded from VSCode. So always use the scss file extension.

## Tailwind

We use [tailwindcss](https://tailwindcss.com/) for global styling. You can configure tailwind at `../src/KDC.Main/tailwind.config.js`.
Most of your CSS should be doable by using tailwinds css classes. So usually you should not need to edit scss files anymore.

Entry point for global styling is `../src/KDC.Main/wwwroot/styles/app.scss`.

## Building CSS files

CSS files are compiles if either the file `./src/KDC.Main/wwwroot/styles/app.css` is missing when launching the app or you manually run:

```bash
cd src/KDC.Main
node build-css.js
```

## Live reload

Live reloading for the app is done by the following commands:

```bash
cd src/KDC.Main
dotnet watch run

# Optional and only needed when working on scss files:
node build-css.js --watch
```

## Scoped / Isolated CSS

All `*.cshtml` files are enabled for isolated styling. How to use:

Assume your file is `foo.cshtml`. Now place a file called `foo.cshtml.scss` next to it.
Any css you place here is applied only to that specific cshtml and cannot affect any other.

> Isolated CSS is also tailwind enabled. So you can use features like @apply.

https://learn.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-8.0&tabs=visual-studio#css-isolation
