# iOS Club 官网前端

## 整体架构

assets: 静态资源
components: 通用组件
layout: 基础模板，这里放的是前台和个人中心的基础模板
pages: 放前台的，个人中心的另起一个文件夹
services: 那些和后端交互的服务
stores: 一些需要长时间使用的东西，例如登录状态
router: 的是路由

## Markdown 解析

这里使用 [markdown-it](https://github.com/markdown-it/markdown-it)

核心代码：
```ts
// 初始化 mermaid
mermaid.initialize({startOnLoad: true})

// 创建 markdown-it 实例
const md = new MarkdownIt({
    html: true,
    linkify: true,
    typographer: true
})

// 使用 anchor 插件
md.use(markdownItAnchor, {
    permalink: true,
    permalinkBefore: true,
    permalinkSymbol: '',
})
md.use(markdownitFootnote)
md.use(markdownitTaskList, {label: false, labelAfter: false})
md.use(markdownitAttrs, {
    allowedAttributes: ['id', 'class', 'target']
})

md.use(mdExpandTabs)
    .use(mdSup)
    .use(mdSub)
    .use(mdMark)

// 配置自定义容器
const containerOptions = [
    {
        name: 'warning',
        className: 'warning'
    },
    {
        name: 'danger',
        className: 'danger'
    },
    {
        name: 'tip',
        className: 'tip'
    }
]

containerOptions.forEach(({name, className}) => {
    md.use(markdownItContainer, name, {
        validate: (params) => {
            return params.trim().match(new RegExp(`^${name}\\s+(.*)$`))
        },
        render: (tokens, idx) => {
            const m = tokens[idx].info.trim().match(new RegExp(`^${name}\\s+(.*)$`))
            if (tokens[idx].nesting === 1) {
                return `<div class="${className} custom-block"><p style="font-weight: bold">${md.utils.escapeHtml(m[1])}</p>\n`
            } else {
                return '</div>\n'
            }
        }
    })
})

// 渲染 markdown 的函数
const render = async (markdown) => {
    const html = md.render(markdown)

    // 等待 DOM 更新
    await nextTick()

    // 初始化 mermaid 图表
    await mermaid.run({
        nodes: document.querySelectorAll('.language-mermaid'),
    })

    // 代码高亮
    setTimeout(() => {
        Prism.highlightAll()
    }, 50)

    return html
}
```
