import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {authInterceptor} from './interceptors';
import {AuthService} from './services/auth.service';

@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ],
  providers:[
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    AuthService
  ]
})
export class CoreModule { }
