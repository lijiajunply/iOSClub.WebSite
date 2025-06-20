import { withMermaid } from "vitepress-plugin-mermaid";
import footnote_plugin from "markdown-it-footnote";

// https://vitepress.dev/reference/site-config
export default withMermaid({
  title: "西建大iOS Club",
  description: "iOS Club of XAUAT",
  head: [
    ['link', { rel: 'icon', sizes: '32x32', href: '/favicon.ico' }],
    ['meta', { name: 'keywords', content: '新生代培养计划, 新生培养, 西建大, iOS Club, 西安建筑科技大学, iOS Club of XAUAT, 培养计划' }],
    ['meta', { name: 'description', content: '西建大 iOS Club 新生代培养计划' }],
    ['meta', { name: 'author', content: 'iOS Club of XAUAT' }],
  ],
  sitemap: {
    hostname: 'https://www.xauat.site'
  },
  themeConfig: {
    sidebarMenuLabel: '目录',
    returnToTopLabel: '回到顶部',
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      {
        text: '关于我们', items: [
          { text: '社团简介', link: '/article/about' },
          { text: '社团结构', link: '/article/structure' },
          { text: '其他组织', link: '/article/otherorg' },
          { text: '竞赛资源', link: '/article/competitions' },
          { text: '社团历史', link: '/history' },
        ]
      },
      {
        text: '社团动态', items: [
          { text: '社团活动', link: '/event' },
          { text: '技术博客', link: '/articles' },
          { text: 'iOS App', link: '/tools' },
          { text: '精品项目', link: '/projects' },
        ]
      },
      { text: '登录/注册', link: '/login' }
    ],

    sidebar: {
      '/article/': [
        {
          text: '社团简介',
          items: [
            { text: '关于我们', link: '/article/about' },
            { text: '社团结构', link: '/article/structure' },
            { text: '其他组织', link: '/article/otherorg' }
          ]
        },
        {
          text: '竞赛资源',
          items: [
            { text: '资源支持', link: '/article/competitions' },
            { text: '移动应用创新赛', link: '/article/mobile-application' },
            { text: 'WWDC-Swift学生挑战赛', link: '/article/swift' }
          ]
        },
        {
          text: '社团活动',
          items: [
            { text: 'iOS Learn', link: '/article/learn' },
            { text: '项目开发活动', link: '/Article/time-to-code' },
            { text: '体验最新产品', link: '/article/vision' },
            { text: '一起看发布会', link: '/article/press-conference' }
          ]
        },
        {
          text: '社团历史',
          items: [
            { text: '总述', link: '/article/history/overview' },
            { text: '历届干部', link: '/article/previous-cadres' },
            { text: '创社史', link: '/article/history/founding' },
            { text: '邵韩之治', link: '/article/history/Shao Han\'s Reign' }
          ]
        }
      ]
    },

    socialLinks: [
      { icon: 'gitee', link: 'https://gitee.com/XAUATiOSClub' },
      { icon: 'github', link: 'https://github.com/XAUAT-iOSClub' }
    ]
  },
  markdown: {
    container: {
      tipLabel: '提示',
      warningLabel: '警告',
      dangerLabel: '危险',
      infoLabel: '信息',
      detailsLabel: '详细信息',
      noteLabel: '注意'
    },
    config(md) {
      md.use(footnote_plugin)
    },
    image: {
      // 默认禁用；设置为 true 可为所有图片启用懒加载。
      lazyLoading: true
    }
  },
  mermaid: {

  },
  // optionally set additional config for plugin itself with MermaidPluginConfig
  mermaidPlugin: {
    class: "mermaid my-class", // set additional css classes for parent container 
  }
})
