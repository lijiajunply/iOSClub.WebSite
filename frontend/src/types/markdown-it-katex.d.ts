declare module 'markdown-it-katex' {
  import MarkdownIt from 'markdown-it';
  
  const katex: (md: MarkdownIt) => void;
  export default katex;
}