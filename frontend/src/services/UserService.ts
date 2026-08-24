import {url} from './Url';
import {apiRequest} from './ApiService';
import {MemberModel} from '../models';

/**
 * 用户服务类 - 处理用户相关的API调用
 */
export class UserService {
    /**
     * 获取当前用户的详细信息
     * @returns Promise<MemberModel> 用户信息对象
     */
    static async getUserData(): Promise<MemberModel> {
        return apiRequest<MemberModel>({
            url: `${url}/User/data`,
            method: 'GET'
        });
    }

    static async updateProfile(memberModel: MemberModel): Promise<void> {
        await apiRequest<void>({
            url: `${url}/User/profile`,
            method: 'PUT',
            body: memberModel
        });
    }
}
