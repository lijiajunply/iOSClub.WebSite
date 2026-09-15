import { url } from './Url';
import { apiRequest } from './ApiService';
import type { StaffModel } from '../models';
export interface RoleUpdate { userId: string; identity: string; departmentName: string | null }
export class FounderPermissionService {
  static members() { return apiRequest<StaffModel[]>({ url: `${url}/FounderPermission/members`, method: 'GET' }) }
  static updateRole(body: RoleUpdate) { return apiRequest<void>({ url: `${url}/FounderPermission/update-role`, method: 'POST', body }) }
}
