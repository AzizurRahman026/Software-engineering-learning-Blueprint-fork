import { Routes } from "@angular/router";

export const DashboardRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import("./components/dashboard-home/dashboard-home").then(o => o.DashboardHome)
    },
    {
        path: 'course',
        loadComponent: () => import('../Courses/Components/subjects-component/subjects-component').then(o => o.SubjectsComponent)
    },
    
    {
        path: 'course/lesson/create',
        loadComponent: () => import('../Courses/Components/lesson-create-component/lesson-create-component').then(o => o.LessonCreateComponent)
    },
    {
        path: 'profile',
        loadComponent: () => import('../Auth/Components/profile/profile.component').then(o => o.ProfileComponent)
    },
    // Blog feature. `create` is listed before `:id` so it isn't captured as a post id.
    {
        path: 'blog',
        loadComponent: () => import('../Blog/Components/blog-list/blog-list').then(o => o.BlogListComponent)
    },
    {
        path: 'blog/create',
        loadComponent: () => import('../Blog/Components/blog-form/blog-form').then(o => o.BlogFormComponent)
    },
    {
        path: 'blog/:id/edit',
        loadComponent: () => import('../Blog/Components/blog-form/blog-form').then(o => o.BlogFormComponent)
    },
    {
        path: 'blog/:id',
        loadComponent: () => import('../Blog/Components/blog-detail/blog-detail').then(o => o.BlogDetailComponent)
    },
];