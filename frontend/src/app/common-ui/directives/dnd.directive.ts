import { Directive, EventEmitter, HostBinding, HostListener, Output } from '@angular/core';

@Directive({
    selector: '[appDnd]',
})
export class DndDirective {
    @HostBinding('class.file-over') fileOver = false;
    @Output() filesDropped = new EventEmitter<File[]>();

    @HostListener('dragover', ['$event'])
    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.fileOver = true;
    }

    @HostListener('dragleave', ['$event'])
    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.fileOver = false;
    }

    @HostListener('drop', ['$event'])
    onDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
        this.fileOver = false;

        const files = Array.from(event.dataTransfer?.files ?? []);
        if (files.length > 0) {
            this.filesDropped.emit(files);
        }
    }
}
