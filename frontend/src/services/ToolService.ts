import {CategoryModel} from "../models";

export class ToolService {
    public static async getTools(): Promise<CategoryModel> {
        return await fetch(`https://link.xauat.site/api/Link/GetCategory/社团出品`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(res => res.json());
    }
}