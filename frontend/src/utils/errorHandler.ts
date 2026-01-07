import { useMessage } from 'naive-ui'
import type { MessageApiInjection } from 'naive-ui/lib/message/src/MessageProvider'
import { getErrorMessage } from '../constants/ErrorCode'

let messageInstance: MessageApiInjection | null = null

export function setMessageInstance(instance: MessageApiInjection) {
    messageInstance = instance
}

export function showError(errorCode: number, errorMessage?: string) {
    if (!messageInstance) {
        console.error('Message instance not initialized:', errorCode, errorMessage)
        return
    }

    const message = errorMessage || getErrorMessage(errorCode)
    messageInstance.error(message)
}

export function showSuccess(message: string) {
    if (!messageInstance) {
        console.log('Success message:', message)
        return
    }
    messageInstance.success(message)
}

export function showWarning(message: string) {
    if (!messageInstance) {
        console.warn('Warning message:', message)
        return
    }
    messageInstance.warning(message)
}

export function showInfo(message: string) {
    if (!messageInstance) {
        console.info('Info message:', message)
        return
    }
    messageInstance.info(message)
}
