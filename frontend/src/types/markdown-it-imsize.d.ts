declare module 'markdown-it-imsize' {
  import MarkdownIt from 'markdown-it';
  
  const imsize: (md: MarkdownIt) => void;
  export default imsize;
}