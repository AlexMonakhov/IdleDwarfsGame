class LogService {

    private log = '';
    private errorLog = '';

    logInfo(message: string): void {
        this.log += message + '\n';
    }

    logError(message: string): void {
        this.errorLog += message + '\n';
    }

    getLog(): string {
        return this.log;
    }

    

    

}

export const logService = new LogService();