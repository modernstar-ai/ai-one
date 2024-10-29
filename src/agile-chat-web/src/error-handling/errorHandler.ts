import { AppError, ErrorContext, ErrorSeverity } from './types';
import { toast } from '@/components/ui/use-toast';

class ErrorHandler {
  private readonly API_ENDPOINT = '/api/logs';

  
  private async logError(error: Error, context?: ErrorContext) {
    const errorLog = {
      timestamp: new Date().toISOString(),
      message: error.message,
      stack: error.stack,
      severity: error instanceof AppError ? error.severity : ErrorSeverity.ERROR,
      errorCode: error instanceof AppError ? error.errorCode : 'UNKNOWN_ERROR',
      context: {
        ...(error instanceof AppError ? error.context : {}),
        ...context,
        userAgent: navigator.userAgent,
        url: window.location.href
      }
    };

    try {
      await fetch(this.API_ENDPOINT, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(errorLog)
      });
    } catch (e) {
      console.error('Failed to send error log:', e);
      this.storeErrorLocally(errorLog);
    }
  }

  private storeErrorLocally(errorLog: unknown) {
    const failedLogs = JSON.parse(localStorage.getItem('failedErrorLogs') || '[]');
    failedLogs.push(errorLog);
    localStorage.setItem('failedErrorLogs', JSON.stringify(failedLogs));
  }

  private showErrorToast(error: Error) {
    const message = error instanceof AppError ? error.message : 'An unexpected error occurred';
    
    toast({
      variant: "destructive",
      title: "Error",
      description: message,
    });
  }

  public handleError(error: Error, context?: ErrorContext): void {
    this.logError(error, context);
    this.showErrorToast(error);
  }

  public createHttpError(status: number, context?: ErrorContext): AppError {
    let errorMessage = 'An unexpected error occurred';
    let severity = ErrorSeverity.ERROR;
    
    switch (status) {
      case 400:
        errorMessage = 'Invalid request. Please check your input.';
        break;
      case 401:
        errorMessage = 'Please log in to continue.';
        break;
      case 403:
        errorMessage = 'You do not have permission to perform this action.';
        break;
      case 404:
        errorMessage = 'The requested resource was not found.';
        break;
      case 500:
        errorMessage = 'Server error. Please try again later.';
        severity = ErrorSeverity.CRITICAL;
        break;
    }

    return new AppError(
      errorMessage,
      severity,
      `HTTP_${status}`,
      context
    );
  }
}

export const errorHandler = new ErrorHandler();
