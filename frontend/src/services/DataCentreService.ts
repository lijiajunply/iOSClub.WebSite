import {url} from './Url';
import {AuthService} from "./AuthService";

// 定义数据模型
export interface YearCount {
    year: string;
    value: number;
}

export interface AcademyCount {
    type: string;
    value: number;
}

export interface GradeCount {
    grade?: string;
    value?: number;
}

export interface GenderCount {
    type: string;
    value: number;
}

// 政治面貌数据可能需要的额外类型
export interface PoliticalCount {
    type: string;
    sales: number;
}

// HTTP请求工具函数
async function fetchData<T>(endpoint: string): Promise<T> {
    try {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }
        const response = await fetch(`${url}/DataCentre/${endpoint}`, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error(`Error fetching ${endpoint}:`, error);
        throw error;
    }
}

// 数据中心服务
export class DataCentreService {
    // 获取历年人数数据
    static async getYearData(): Promise<YearCount[]> {
        return fetchData<YearCount[]>('year');
    }

    // 获取学院分布数据
    static async getCollegeData(): Promise<AcademyCount[]> {
        return fetchData<AcademyCount[]>('college');
    }

    // 获取年级分布数据
    static async getGradeData(): Promise<GradeCount[]> {
        return fetchData<GradeCount[]>('grade');
    }

    // 获取政治面貌分布数据
    static async getLandscapeData(): Promise<PoliticalCount[]> {
        return fetchData<PoliticalCount[]>('landscape');
    }

    // 获取性别分布数据
    static async getGenderData(): Promise<GenderCount[]> {
        return fetchData<GenderCount[]>('gender');
    }

    static async getCentreData(): Promise<any> {
        return fetchData<any>('');
    }

    static async updateDataFromJson(file: File): Promise<void> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }
        const response = await fetch(`${url}/DataCentre/update-from-json`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
            body: file,
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
    }

    static async exportJson(): Promise<Blob> {
        const token = AuthService.getToken();
        if (!token) {
            throw new Error('未登录');
        }
        return await fetch(`${url}/DataCentre/export-json`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
        }).then(response => response.blob());
    }
}