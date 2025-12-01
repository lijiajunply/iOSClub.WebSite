import {ToolCategoryModel} from "../models";

export class ToolService {
    public static async getTools(): Promise<ToolCategoryModel> {
        return await fetch(`https://link.xauat.site/Category/byName/社团出品`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(res => res.json());
    }
}