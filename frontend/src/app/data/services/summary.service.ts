import { HttpClient } from '@angular/common/http';
import { inject, Service, signal } from '@angular/core';
import { finalize, pipe, Subject, takeUntil, tap } from 'rxjs';

@Service()
export class SummaryService {
    private readonly httpClient = inject(HttpClient);
    private readonly baseUrl = "/api/summary";
    readonly isProcessing = signal<boolean>(false);
    private readonly cancel$ = new Subject<void>();

    processFile(file: File) {
        this.cancelProcessing();
        this.isProcessing.set(true);

        const formData = new FormData();
        formData.append('file', file);

        return this.httpClient.post(
            `${this.baseUrl}/generate`, formData, {
                responseType: 'blob',
                observe: 'response'
            }).pipe(
            takeUntil(this.cancel$),
            finalize(() => this.isProcessing.set(false))
        );
    }

    cancelProcessing() {
        this.cancel$.next();
    }
}
