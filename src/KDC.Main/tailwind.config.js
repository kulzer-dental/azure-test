/** @type {import('tailwindcss').Config} */

var isDevelopment = process.env.ASPNETCORE_ENVIRONMENT === "Development";

module.exports = {
  darkMode: "media",
  content: [
    "./**/*.{razor,html,cshtml,scss}",
    "./wwwroot/lib/flowbite/**/*.js",
  ],
  safelist: isDevelopment
    ? [
        {
          pattern: /./,
        },
      ]
    : [],
  extend: {},
  theme: {
    input: {
    },
    colors: {
      primary: {
        "50": "#fffee4",
        "100": "#fffec5",
        "200": "#fdff92",
        "300": "#f7ff53",
        "400": "#ebfb20",
        "500": "#bccf00", // Styleguide
        "600": "#9fb500",
        "700": "#788902",
        "800": "#5e6c08",
        "900": "#4f5b0c",
        "950": "#293300",
      },
      slate: {
        "50": "#fffee4",
        '50': '#f6f6f5',
        '100': '#e7e7e6',
        '200': '#d1d1d0',
        '300': '#b0b0b0',
        '400': '#888987',
        '500': '#6d6e6c', // Styleguide
        '600': '#575756',
        '700': '#4f4f4f',
        '800': '#454545',
        '900': '#3d3d3c',
        '950': '#262626',
      },
      grey: {
        '50': '#f6f6f5',
        '100': '#e7e7e6',
        '200': '#d1d1d0',
        '300': '#b0b0b0',
        '400': '#919190',
        '500': '#6d6e6c', // Styleguide
        '600': '#5d5d5d',
        '700': '#4f4f4f',
        '800': '#454545',
        '900': '#3d3d3c',
        '950': '#262626',
      },
    },
  },
  fontFamily: {
    body: [
      "Inter",
      "ui-sans-serif",
      "system-ui",
      "-apple-system",
      "system-ui",
      "Segoe UI",
      "Roboto",
      "Helvetica Neue",
      "Arial",
      "Noto Sans",
      "sans-serif",
      "Apple Color Emoji",
      "Segoe UI Emoji",
      "Segoe UI Symbol",
      "Noto Color Emoji",
    ],
    sans: [
      "Inter",
      "ui-sans-serif",
      "system-ui",
      "-apple-system",
      "system-ui",
      "Segoe UI",
      "Roboto",
      "Helvetica Neue",
      "Arial",
      "Noto Sans",
      "sans-serif",
      "Apple Color Emoji",
      "Segoe UI Emoji",
      "Segoe UI Symbol",
      "Noto Color Emoji",
    ],
  },
  plugins: [
    require("flowbite/plugin"),
    function ({ addVariant }) {
      /**
       * If you have a .light class
       */
      addVariant('light', '.light &')
      
      /**
       * If you only have .dark to work with, simply swap out
       * `html` in the example below with the parent tag where
       * you are applying the .dark class
       */
      addVariant('light', 'html:not(.dark) &')

      /**
       * Uses system default preference.
       */
      addVariant('light', '@media (prefers-color-scheme: light)')
    },
  ],
};
