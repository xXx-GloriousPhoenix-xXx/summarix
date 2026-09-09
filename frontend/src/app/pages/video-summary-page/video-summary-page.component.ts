import { Component, signal } from '@angular/core';
import { DndDirective } from "../../common-ui/directives/dnd.directive";

@Component({
    imports: [DndDirective],
    selector: 'app-video-summary-page',
    styleUrl: './video-summary-page.component.scss',
    templateUrl: './video-summary-page.component.html',
})
export class VideoSummaryPageComponent {
    protected readonly files = signal<File[]>([]);
    protected readonly isProcessing = signal<boolean>(false);

    setFiles(files: File[]) {
        console.log(files);
        this.files.set(files);
    }
}
