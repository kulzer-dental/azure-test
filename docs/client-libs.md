# Client Libraries

When using client side libraries like jquery, they mut be placed in the wwwroot directory. This is the only place where static files can be loaded.

## Package manager

We use npm as our package manager. All packages are managed in package.json. It's important to make sure, that client side libs are installed as dependencies and dev libs (compile time) as devDependencies.

### Installing dependencies

Install dependencies using `npm ci` or `npm i`.

#### MS Build integration

When building the app while no *.js files can be found at `./src/KDC.Main/wwwroot/lib/**/*.js` this will trigger the gulp copy job by running `npx gulp copy`. 

### Adding a dev dependency

From the repository root directory run:

`npm install LIBNAME -D`

### Adding a client side dependency

From the repository root directory run:

`npm install LIBNAME`

Now open `../src/KDC.Main/gulpfile.js` and add a copy function for your new lib. Here is an example for flowbite:

```js
function copyFlowbite(cb) {
  return gulp
    .src(["../../node_modules/flowbite/dist/**/*"]) // Path of needed filed
    .pipe(gulp.dest("./wwwroot/lib/flowbite")); // Path for public static files
}
```

Then add the new function to the series in the end:

```js
exports.copy = series(
  copyFlowbite,
  ...
);
```

The copy job is executed everytime you run `npm install`or `npm ci`. This makes sure, that the package.json version and the installed lib stays in sync.

You can trigger the job manually by running:

```bash
cd src/KDC.Main
npx gulp copy
```

### Clean the client side libs

To delete all client side libs from wwwroot run this:

```bash
cd src/KDC.Main
npx gulp clean
```
