import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { 
  HasRoleDirective,
  HasNotRoleDirective,
  HasAnyRoleDirective,
  HasAllRolesDirective
} from './directives';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { NotAuthorizedComponent } from './components/not-authorized/not-authorized.component';

@NgModule({
  declarations: [
    HasRoleDirective,
    HasNotRoleDirective,
    HasAnyRoleDirective,
    HasAllRolesDirective,
    NotFoundComponent,
    NotAuthorizedComponent
  ],
  imports: [
    CommonModule,
    RouterModule
  ],
  exports: [
    CommonModule,
    HasRoleDirective,
    HasNotRoleDirective,
    HasAnyRoleDirective,
    HasAllRolesDirective,
    NotFoundComponent,
    NotAuthorizedComponent
  ]
})
export class SharedModule { }
