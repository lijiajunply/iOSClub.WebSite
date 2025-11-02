import { url } from './Url';

// 定义数据模型
export interface YearCount {
  year: string;
  value: number;
}

export interface AcademyCount {
  type: string;
  value: number;
}

export interface LandscapeCount {
  type: string;
  value: number;
  // 年级数据可能使用的字段
  年级?: string;
  人数?: number;
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
    const response = await fetch(`${url}/DateCentre/${endpoint}`);
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
export class DateCentreService {
  // 获取历年人数数据
  static async getYearData(): Promise<YearCount[]> {
    return fetchData<YearCount[]>('year');
  }

  // 获取学院分布数据
  static async getCollegeData(): Promise<AcademyCount[]> {
    return fetchData<AcademyCount[]>('college');
  }

  // 获取年级分布数据
  static async getGradeData(): Promise<LandscapeCount[]> {
    return fetchData<LandscapeCount[]>('grade');
  }

  // 获取政治面貌分布数据
  static async getLandscapeData(): Promise<LandscapeCount[]> {
    return fetchData<LandscapeCount[]>('landscape');
  }

  // 获取性别分布数据
  static async getGenderData(): Promise<GenderCount[]> {
    return fetchData<GenderCount[]>('gender');
  }
}