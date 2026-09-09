import { Routes } from '@angular/router';
import { HomeLayoutComponent } from './pages/home-layout/home-layout.component';
import { VideoSummaryPageComponent } from './pages/video-summary-page/video-summary-page.component';

export const routes: Routes = [
    { path: "", redirectTo: "home/video-summary", pathMatch: "full" },
    {
        path: "home",
        component: HomeLayoutComponent,
        children: [
            { path: "video-summary", component: VideoSummaryPageComponent }
        ]
    }
];
