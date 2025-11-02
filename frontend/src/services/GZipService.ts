import pako from 'pako';

/**
 * GZip 服务类 - 处理 gzip 压缩和解压缩功能
 */
export class GZipService {
    static async compressFromUint8Array(data: string): Promise<Uint8Array> {
        try {
            return pako.gzip(data, {level: 9});
        } catch (error: any) {
            throw new Error(`压缩数据失败: ${error.message}`);
        }
    }
    
    static async decompressFromUint8Array(compressedData: Uint8Array): Promise<string> {
        try {
            return pako.inflate(compressedData, {to: 'string'});
        } catch (error: any) {
            throw new Error(`解压数据失败: ${error.message}`);
        }
    }
    
    static async compressFromString(data: string): Promise<string> {
        try {
            const compressedData = pako.gzip(data, {level: 9});
            // 使用浏览器原生 API 进行 base64 编码
            return btoa(String.fromCharCode(...compressedData));
        } catch (error: any) {
            throw new Error(`压缩数据失败: ${error.message}`);
        }
    }
    
    static async decompressFromString(compressedData: string): Promise<string> {
        try {
            // 使用浏览器原生 API 进行 base64 解码
            const compressedBuffer = new Uint8Array(atob(compressedData).split('').map(c => c.charCodeAt(0)));
            return pako.inflate(compressedBuffer, {to: 'string'});
        } catch (error: any) {
            throw new Error(`解压数据失败: ${error.message}`);
        }
    }
}