import { Url } from './Url';

export interface ArticleModel {
    path: string;
    title: string;
    content: string;
    lastWriteTime: string;
    identity: string;
    watch: number;
}

export interface ArticleProps {
    title: string;
    date: string;
    watch: number;
    content: string;
}

export class ArticleService {
    public static async getArticles(): Promise<any> {
        return await fetch(`${Url}/Article`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(res => res.json());
    }

    public static async getArticle(path: string): Promise<ArticleModel> {
        return await fetch(`${Url}/Article/${path}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(res => res.json());
    }

    public static async addArticle(article: any): Promise<any> {
        return await fetch(`${Url}/Articles`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(article)
        }).then(res => res.json());
    }
}