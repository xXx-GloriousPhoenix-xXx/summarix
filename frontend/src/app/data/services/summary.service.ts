import { HttpClient } from '@angular/common/http';
import { inject, Service, signal } from '@angular/core';
import { finalize, pipe, Subject, takeUntil } from 'rxjs';

@Service()
export class SummaryService {
    private readonly httpClient = inject(HttpClient);
    private readonly baseUrl = "/api/summary";
    readonly isProcessing = signal<boolean>(false);
    private cancel$?: Subject<void>;

    processFile(file: File, language: string = 'en') {
        if (this.isProcessing()) {
            this.cancelProcessing();
        }

        this.isProcessing.set(true);
        this.cancel$ = new Subject<void>();

        const formData = new FormData();
        formData.append('File', file, file.name);
        formData.append('Language', language);

        return this.httpClient.post(
            `${this.baseUrl}/generate`, formData, {
                responseType: 'blob',
                observe: 'response'
            }).pipe(
            takeUntil(this.cancel$),
            finalize(() => {
                this.isProcessing.set(false);
                this.cancel$ = undefined;
            })
        );
    }

    cancelProcessing(): void {
        if (this.cancel$) {
          this.cancel$.next();
          this.cancel$.complete();
          this.cancel$ = undefined;
        }
        this.isProcessing.set(false);
    }
}
