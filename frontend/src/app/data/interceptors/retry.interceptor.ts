import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { retry, throwError, timer } from 'rxjs';

const RETRYABLE_STATUS_CODES = [
    500, // Internal Server Error
    502, // Bad Gateway (TranscriptionFailed / SummarizationFailed)
    503, // Service Unavailable
    504  // Gateway Timeout
  ];

export const retryInterceptor: HttpInterceptorFn = (req, next) => {
    return next(req).pipe(
        retry({
            count: 2,
            delay: (error: any, retryCount: number) => {
                const isNetworkError = error instanceof HttpErrorResponse && error.status === 0;
                const isServerRetryable = error instanceof HttpErrorResponse && RETRYABLE_STATUS_CODES.includes(error.status);
        
                if (isNetworkError || isServerRetryable) {
                    const delayMs = retryCount * 1000;
                    console.warn(`[RetryInterceptor] Запрос ${req.url} завершился ошибкой (${error.status}). Повтор #${retryCount} через ${delayMs}мс...`);
                    return timer(delayMs);
                }
        
                return throwError(() => error);
            }
        })
    );
};
