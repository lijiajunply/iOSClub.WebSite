export class LinkModel {
    public key: string = "";
    public name: string = "";
    public icon: string = ""; // 原C#中为string?但初始化为空字符串，TS中无需可选
    public url: string = "";
    public description: string = ""; // 同上
    public index: number = 0; // C#中int默认0，TS显式初始化
}

export class CategoryModel {
    public key: string = "";
    public name: string = "";
    public description: string = "";
    public icon: string = "";
    public index: number = 0;
    public links: LinkModel[] = []; // 原List<LinkModel>转换为TS数组
}