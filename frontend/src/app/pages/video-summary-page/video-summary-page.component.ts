import { Component, HostListener, inject, signal } from '@angular/core';
import { DndDirective } from "../../common-ui/directives/dnd.directive";
import { SummaryService } from '../../data/services/summary.service';

@Component({
    imports: [DndDirective],
    selector: 'app-video-summary-page',
    styleUrl: './video-summary-page.component.scss',
    templateUrl: './video-summary-page.component.html',
})
export class VideoSummaryPageComponent {
    private readonly summaryService = inject(SummaryService);

    protected readonly videoFile = signal<File | null>(null);
    protected readonly isProcessing = signal<boolean>(false);
    protected readonly pdfFile = signal<File | null>(null);

    protected setFiles(files: File[]) {
        if (files.at(0)) {
            this.videoFile.set(files[0]);
        }
    }

    @HostListener('window:paste', ['$event'])
    protected onPaste(event: ClipboardEvent) {
        const clipboardData = event.clipboardData;
        if (!clipboardData) return;

        const files = Array.from(clipboardData.files);
        const video = files.find(file => file.type.startsWith('video/'));

        if (video) {
            event.preventDefault();
            this.videoFile.set(video);
        }
    }

    protected onProcess() {
        const currentFile = this.videoFile();
        if (currentFile === null) {
            return;
        }

        this.summaryService.processFile(currentFile).subscribe({
            next: (response) => {
                const contentType = response.headers.get("Content-Type");
                const blob = response.body;

                const isPdf = contentType?.includes('application/pdf') || blob?.type === 'application/pdf';
                if (isPdf && blob) {
                    const fileUrl = URL.createObjectURL(blob);
                    window.open(fileUrl);
                }
                else {
                    console.warn("Response format is not PDF");
                }
            },
            error: async (err) => {
                if (err.error instanceof Blob) {
                    const errorText = await err.error.text();
                    console.error("Error:", errorText);
                }
            }
        });
    }

    protected onCancel() {
        this.summaryService.cancelProcessing();
    }

    protected onRemove() {
        this.videoFile.set(null);
    }

    protected onDownload() {

    }

    protected onFileSelected(event: Event) {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];

        if (file && file.type.startsWith('video/')) {
            this.videoFile.set(file);
        }

        input.value = '';
    }
}
