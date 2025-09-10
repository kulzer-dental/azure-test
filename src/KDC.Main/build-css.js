const { globSync } = require("glob");
const { exec, spawn } = require("child_process");

const watch = process.argv.slice(2).includes("--watch");

const srcPath = __dirname.slice(0, __dirname.indexOf("src") + 3);

console.log("Building CSS files...");

const scssFiles = globSync(`${srcPath}/**/*.scss`);
scssFiles.forEach((file) => {
  const command = `npx postcss "${file}" -o "${file.replace(
    ".scss",
    ".css",
    ".cshtml"
  )}" --config "${__dirname}/postcss.config.js" ${
    watch ? "--watch --verbose" : ""
  }`;

  if (watch) {
    spawn(command, { shell: true, stdio: "inherit" });
  } else {
    exec(command, (error, stdout, stderr) => {
      if (error) {
        console.error(`exec error: ${error}`);
      }
    });
  }
});

console.log("Done building CSS files");
