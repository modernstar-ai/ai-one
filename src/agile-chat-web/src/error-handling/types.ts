export enum ErrorSeverity {
    INFO = 'INFO',
    WARNING = 'WARNING',
    ERROR = 'ERROR',
    CRITICAL = 'CRITICAL'
  }
  
  // Updated ErrorContext interface with all optional properties
  export interface ErrorContext {
    componentName?: string | undefined;
    action?: string | undefined;
    userId?: string | undefined;
    additionalData?: Record<string, unknown> | undefined;
  }
  
  export class AppError extends Error {
    constructor(
      message: string,
      public severity: ErrorSeverity = ErrorSeverity.ERROR,
      public errorCode?: string,
      public context?: ErrorContext
    ) {
      super(message);
      this.name = 'AppError';
    }
  }