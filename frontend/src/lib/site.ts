export const isWeiXin = () => {
    const ua = navigator.userAgent
    return /MicroMessenger/i.test(ua)
}

export const NavigateTo = (url: string, webUrl: string) => {
    const u = navigator.userAgent;
    const isAndroid = u.indexOf('Android') > -1 || u.indexOf('Adr') > -1; //android终端
    const isiOS = !!u.match(/\(i[^;]+;( U;)? CPU.+Mac OS X/); //ios终端
    let result = isAndroid || isiOS;
    if (u.indexOf('MQQBrowser') !== -1 || !result) {
        window.open(webUrl)
    } else {
        window.open(url)
    }
}