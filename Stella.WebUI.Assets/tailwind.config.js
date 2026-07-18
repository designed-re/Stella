/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: "class",
  content: [
    "../Stella/WebUI/Pages/**/*.cshtml",
    "../StellaKFCPlugin/WebUI/Pages/**/*.cshtml",
    "../StellaKFCPlugin/wwwroot/webui/**/*.{html,js}",
    "../Stella/wwwroot/webui/**/*.js",
  ],
  theme: {
    extend: {
      colors: {
        accent: {
          50: "#fdf2f8", 100: "#fce7f3", 200: "#fbcfe8", 300: "#f9a8d4",
          400: "#f472b6", 500: "#ec4899", 600: "#db2777", 700: "#be185d",
        },
      },
    },
  },
};
