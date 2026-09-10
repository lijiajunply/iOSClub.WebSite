import {url} from './Url';
import {apiRequest} from './ApiService';
import {MemberVO, StudentUpdateDTO} from '../models';

/**
 * 用户服务类 - 处理用户相关的API调用
 */
export class UserService {
    /**
     * 获取当前用户的详细信息
     * @returns Promise<MemberVO> 用户信息对象
     */
    static async getUserData(): Promise<MemberVO> {
        return apiRequest<MemberVO>({
            url: `${url}/User/data`,
            method: 'GET'
        });
    }

    static async updateProfile(model: StudentUpdateDTO): Promise<void> {
        await apiRequest<void>({
            url: `${url}/User/profile`,
            method: 'PUT',
            body: model
        });
    }
}
