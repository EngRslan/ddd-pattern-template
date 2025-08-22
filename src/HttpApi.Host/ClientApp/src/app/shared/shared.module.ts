import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { 
  HasRoleDirective,
  HasNotRoleDirective,
  HasAnyRoleDirective,
  HasAllRolesDirective
} from './directives';

@NgModule({
  declarations: [
    HasRoleDirective,
    HasNotRoleDirective,
    HasAnyRoleDirective,
    HasAllRolesDirective
  ],
  imports: [
    CommonModule
  ],
  exports: [
    CommonModule,
    HasRoleDirective,
    HasNotRoleDirective,
    HasAnyRoleDirective,
    HasAllRolesDirective
  ]
})
export class SharedModule { }
