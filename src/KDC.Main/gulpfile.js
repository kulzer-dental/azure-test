var gulp = require("gulp");
var del = require("del");
const { series } = require("gulp");
const fs = require("fs");
var path = require("path");

function clean(cb) {
  return del(["wwwroot/lib", "wwwroot/styles/app.css"]);
}

function copyFlowbite(cb) {
  return gulp
    .src(["../../node_modules/flowbite/dist/**/*"])
    .pipe(gulp.dest("./wwwroot/lib/flowbite"));
}

function copyJquery(cb) {
  return gulp
    .src(["../../node_modules/jquery/dist/**/*"])
    .pipe(gulp.dest("./wwwroot/lib/jquery"));
}

function copyJqueryValidation(cb) {
  return gulp
    .src(["../../node_modules/jquery-validation/dist/**/*"])
    .pipe(gulp.dest("./wwwroot/lib/jquery-validation"));
}

function copyJqueryValidationUnobtrusive(cb) {
  return gulp
    .src(["../../node_modules/jquery-validation-unobtrusive/dist/**/*"])
    .pipe(gulp.dest("./wwwroot/lib/jquery-validation-unobtrusive"));
}

function copyQrCodeJs(cb) {
  return gulp
    .src(["../../node_modules/qrcodejs/**/*"])
    .pipe(gulp.dest("./wwwroot/lib/qrcodejs"));
}


exports.clean = series(clean);
exports.copy = series(
  copyFlowbite,
  copyJquery,
  copyJqueryValidation,
  copyJqueryValidationUnobtrusive,
  copyQrCodeJs
);
