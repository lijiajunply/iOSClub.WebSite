declare module 'markdown-it' {
  // 定义 MarkdownIt 类型
  interface MarkdownIt {
    render(src: string, env?: any): string;
    renderInline(src: string, env?: any): string;
    parse(src: string, env?: any): any[];
    use(plugin: any, options?: any): this;
    set(options: any): this;
    get(key: string): any;
    enable(list: string | string[], ignoreInvalid?: boolean): this;
    disable(list: string | string[], ignoreInvalid?: boolean): this;
    toggle(list: string | string[], enable?: boolean, ignoreInvalid?: boolean): this;
    isEnabled(name: string): boolean;
  }

  // 定义 Renderer 命名空间
  namespace MarkdownIt {
    namespace Renderer {
      type RenderRule = (tokens: any[], idx: number, options: any, env: any, self: any) => string;
    }
  }

  // 定义构造函数
  function MarkdownIt(options?: any): MarkdownIt;
  function MarkdownIt(presetName: string, options?: any): MarkdownIt;

  export = MarkdownIt;
}