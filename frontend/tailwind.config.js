/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    '!./node_modules',
    '!./dist',
    './docs/**/*.{html,js,md,mdx}',
    './docs/.vitepress/**/*.{html,js,md,mdx}',
    './docs/.vitepress/theme/**/*.{html,js,md,mdx}',
    './docs/.vitepress/components/**/*.{html,js,md,mdx,vue}',
  ],
  theme: {
    extend: {},
  },
  darkMode: 'class',
  plugins: [],
}

