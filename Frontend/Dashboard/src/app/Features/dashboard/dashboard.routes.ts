import { Routes } from "@angular/router";
import { adminGuard } from "../../Core/Guards/admin.guard";

export const DashboardRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import("./components/dashboard-home/dashboard-home").then(o => o.DashboardHome)
    },
    {
        path: 'profile',
        loadComponent: () => import('../Auth/Components/profile/profile.component').then(o => o.ProfileComponent)
    },
    {
        path: 'admin',
        canActivate: [adminGuard],
        loadComponent: () => import('../Admin/admin.component').then(o => o.AdminComponent)
    },
    // Posts feature. `create` is listed before `:id` so it isn't captured as a post id.
    {
        path: 'posts',
        loadComponent: () => import('../Posts/Components/post-list/post-list').then(o => o.PostListComponent)
    },
    {
        path: 'posts/create',
        loadComponent: () => import('../Posts/Components/post-form/post-form').then(o => o.PostFormComponent)
    },
    {
        path: 'posts/mine',
        loadComponent: () => import('../Posts/Components/my-posts/my-posts').then(o => o.MyPostsComponent)
    },
    {
        path: 'posts/:id/edit',
        loadComponent: () => import('../Posts/Components/post-form/post-form').then(o => o.PostFormComponent)
    },
    {
        path: 'posts/:id',
        loadComponent: () => import('../Posts/Components/post-detail/post-detail').then(o => o.PostDetailComponent)
    },
];
